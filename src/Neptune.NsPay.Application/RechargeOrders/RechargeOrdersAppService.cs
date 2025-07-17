using Abp.Authorization;
using Abp.Domain.Repositories;
using MailKit.Search;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.MerchantRates;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RechargeOrders.Dtos;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RechargeOrders
{
    [AbpAuthorize(AppPermissions.Pages_RechargeOrders)]
    public class RechargeOrdersAppService: NsPayAppServiceBase, IRechargeOrdersAppService
    {
        private readonly IRepository<Merchant> _merchantRepository;
        private readonly IRepository<MerchantRate> _merchantRateRepository;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IRedisService _redisService;
        private readonly IKafkaProducer _kafkaProducer;

        public RechargeOrdersAppService(IPayOrdersMongoService payOrdersMongoService,
            IRepository<Merchant> merchantRepository,
            IRepository<MerchantRate> merchantRateRepository,
            IRedisService redisService,
            IKafkaProducer kafkaProducer)
        {
            _payOrdersMongoService = payOrdersMongoService;
            _merchantRepository = merchantRepository;
            _merchantRateRepository = merchantRateRepository;
            _redisService = redisService;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<IList<GetOrderMerchantViewDto>> GetMerchants()
        {
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants.Select(r => new GetOrderMerchantViewDto
            {
                MerchantCode = r.MerchantCode,
                MerchantName = r.Name
            }).ToList();
            return merchants;
        }

        [AbpAuthorize(AppPermissions.Pages_RechargeOrders_Create)]
        public async Task CreateRecharge(CreateRechargeOrdersDto input)
        {
            if (input != null)
            {
                if(!string.IsNullOrEmpty(input.MerchantCode))
                {
                    var merchant = await _merchantRepository.FirstOrDefaultAsync(r => r.MerchantCode == input.MerchantCode);
                    if (merchant != null)
                    {
                        var merchantRate = await _merchantRateRepository.FirstOrDefaultAsync(r => r.MerchantCode == input.MerchantCode);
                        if (merchantRate != null)
                        {
                            //订单生成
                            var rate = merchantRate.ScanBankRate;
                            var feeMoney = GetFeeMoney(input.OrderMoney, rate);
                            if (input.ComputeRate == 2)
                            {
                                rate = 0;
                                feeMoney = 0;
                            }
                            var dateTime = DateTime.Now;
                            PayOrdersMongoEntity payOrdersMongoEntity = new PayOrdersMongoEntity()
                            {
                                MerchantCode = merchant.MerchantCode,
                                MerchantId = merchant.Id,
                                OrderNumber = Guid.NewGuid().ToString("N"),
                                OrderNo = Guid.NewGuid().ToString("N"),
                                OrderType = PayOrderOrderTypeEnum.TopUp,
                                OrderStatus = PayOrderOrderStatusEnum.Completed,
                                OrderMoney = input.OrderMoney,
                                Rate = rate,
                                FeeMoney = feeMoney,
                                OrderMark = "TopUp",
                                PlatformCode = merchant.PlatformCode,
                                PayMentId = 0,
                                UserId = "",
                                UserNo = "",
                                NotifyUrl = "",
                                PayType = PayMentTypeEnum.NoPay,
                                ScoreStatus = PayOrderScoreStatusEnum.Completed,
                                ScoreNumber = 0,
                                PaymentChannel = PaymentChannelEnum.OnlineBank,
                                MerchantType = merchant.MerchantType,
                                Remark = input.Remark,
                                TransactionTime = dateTime,
                                TransactionUnixTime = TimeHelper.GetUnixTimeStamp(dateTime)
                            };

                            var orderId = await _payOrdersMongoService.AddAsync(payOrdersMongoEntity);

                            //添加流水
                            var checkOrder = _redisService.GetMerchantBillOrder(payOrdersMongoEntity.MerchantCode, payOrdersMongoEntity.OrderNumber);
                            if (string.IsNullOrEmpty(checkOrder))
                            {
                                _redisService.SetMerchantBillOrder(payOrdersMongoEntity.MerchantCode, payOrdersMongoEntity.OrderNumber);
                                //PayMerchantRedisMqDto redisMqDto = new PayMerchantRedisMqDto()
                                //{
                                //    PayMqSubType = MQSubscribeStaticConsts.MerchantBillAddBalance,
                                //    MerchantCode = payOrdersMongoEntity.MerchantCode,
                                //    PayOrderId = orderId,
                                //};
                                //_redisService.SetMerchantMqPublish(redisMqDto);

                                var payOrder = new PayOrderPublishDto()
                                {
                                    MerchantCode = payOrdersMongoEntity.MerchantCode,
                                    PayOrderId = orderId,
                                    TriggerDate = DateTime.Now,
                                    ProcessId = Guid.NewGuid().ToString()
                                };
                                await _kafkaProducer.ProduceAsync<PayOrderPublishDto>(KafkaTopics.PayOrder, orderId, payOrder);

                            }

                        }
                    }
                }
            }
        }

        public static decimal GetFeeMoney(decimal money, decimal rate)
        {
            return (rate / 100) * money;
        }

        public static decimal GetRate(MerchantRateRedisModel merchant, PayMentTypeEnum paytype)
        {
            if (paytype == PayMentTypeEnum.ScratchCards)
            {
                return merchant.ScratchCardRate;
            }
            else if (paytype == PayMentTypeEnum.MoMoPay)
            {
                return merchant.MoMoRate;
            }
            return merchant.ScanBankRate;
        }


    }
}

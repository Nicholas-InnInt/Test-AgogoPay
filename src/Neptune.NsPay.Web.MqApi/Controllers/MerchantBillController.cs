using Abp.Collections.Extensions;
using Abp.Extensions;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.BillingExtensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RabbitMqExtensions.Models;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.WithdrawalOrders;
using System.Diagnostics;
using System.Threading;

namespace Neptune.NsPay.Web.MqApi.Controllers
{
    public class MerchantBillController : Controller
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IMerchantBillsMongoService _merchantBillsMongoService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IMerchantWithdrawService _merchantWithdrawService;
        private readonly IRedisService _redisService;
        private readonly IMerchantBillsHelper _merchantBillsHelper;
        private readonly ILogger<MerchantBillController> _logger;
        public MerchantBillController(IPayOrdersMongoService payOrdersMongoService,
            IMerchantBillsMongoService merchantBillsMongoService,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IMerchantWithdrawService merchantWithdrawService,
            IRedisService redisService,
            IMerchantBillsHelper merchantBillsHelper,
            ILogger<MerchantBillController> logger)
        {
            _payOrdersMongoService = payOrdersMongoService;
            _merchantBillsMongoService = merchantBillsMongoService;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _merchantWithdrawService = merchantWithdrawService;
            _redisService = redisService;
            _merchantBillsHelper = merchantBillsHelper;
            _logger = logger;
        }

        /// <summary>
        /// 代收订单
        /// </summary>
        /// <param name="mqDto"></param>
        /// <returns></returns>
        [CapSubscribe(MQSubscribeStaticConsts.MerchantBillAddBalance)]
        public async Task MerchantAddBalance(PayMerchantMqDto mqDto)
        {
            try
            {
                _logger.LogInformation("MerchantAddBalance Start - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + " Order Number - " + mqDto!=null?mqDto.PayOrderId:string.Empty);
                if (mqDto != null)
                {
                    if (!mqDto.MerchantCode.IsNullOrEmpty() && !mqDto.PayOrderId.IsNullOrEmpty())
                    {
                        var cacheKey = $"merchant-add-balance:{mqDto.MerchantCode}:{mqDto.PayOrderId}";
                        var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
                        if (isProcessed)
                        {
                            return; // 直接返回，避免重复处理
                        }

                        var payOrder = await _payOrdersMongoService.GetPayOrderByOrderId(mqDto.PayOrderId);
                        if (payOrder != null)
                        {
                            if (payOrder.OrderStatus == PayOrderOrderStatusEnum.Completed)
                            {
                                var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(mqDto.MerchantCode, payOrder.OrderNumber, MerchantBillTypeEnum.OrderIn);
                                if (info == null)
                                {
                                    var checkOrder = _redisService.GetMerchantMqBillOrder(payOrder.MerchantCode, payOrder.ID);
                                    if (checkOrder.IsNullOrEmpty())
                                    {
                                        //using (_redisService.GetMerchantBalanceLock(mqDto.MerchantCode)) // avoiding same merchant update balance by multiple part of code
                                        //{
                                        var result = await _merchantBillsHelper.AddRetryPayOrderBillAsync(mqDto.MerchantCode, payOrder.ID);
                                        if (result)
                                        {
                                            _redisService.SetMerchantMqBillOrder(payOrder.MerchantCode, payOrder.ID);
                                        }
                                        //}
                                    }
                                }
                            }
                        }

                        // 在 Redis 中标记订单已处理
                        _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间，防止长期锁定
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("商户代收订单流水异常:" + ex);
                throw;
            }

        }
        
        /// <summary>
        /// 代付订单
        /// </summary>
        /// <param name="mqDto"></param>
        /// <returns></returns>
        [CapSubscribe(MQSubscribeStaticConsts.MerchantBillReduceBalance)]
        public async Task MerchantReduceBalance(TransferMerchantMqDto mqDto)
        {
            try
            {
                _logger.LogInformation("MerchantReduceBalance Start - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + " Order Number - " + mqDto != null ? mqDto.WithdrawalOrderId : string.Empty);

                if (mqDto != null)
                {
                    if(!mqDto.MerchantCode.IsNullOrEmpty() && !mqDto.WithdrawalOrderId.IsNullOrEmpty())
                    {
                        var cacheKey = $"merchant-reduce-balance:{mqDto.MerchantCode}:{mqDto.WithdrawalOrderId}";
                        var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
                        if (isProcessed)
                        {
                            return; // 直接返回，避免重复处理
                        }

                        var withdrawOrder = await _withdrawalOrdersMongoService.GetWithdrawOrderByOrderId(mqDto.MerchantCode,mqDto.WithdrawalOrderId);
                        if(withdrawOrder != null)
                        {
                            if(withdrawOrder.OrderStatus== WithdrawalOrderStatusEnum.Success)
                            {
                                var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(mqDto.MerchantCode, withdrawOrder.OrderNumber, MerchantBillTypeEnum.Withdraw);
                                if (info == null)
                                {
                                    var checkOrder = _redisService.GetMerchantMqBillOrder(withdrawOrder.MerchantCode, withdrawOrder.ID);
                                    if (checkOrder.IsNullOrEmpty())
                                    {
                                        //using (_redisService.GetMerchantBalanceLock(mqDto.MerchantCode)) // avoiding same merchant update balance by multiple part of code
                                        //{
                                            var result = await _merchantBillsHelper.AddRetryWithdrawalOrderBillAsync(mqDto.MerchantCode, withdrawOrder.ID);
                                            if (result)
                                            {
                                                _redisService.SetMerchantMqBillOrder(withdrawOrder.MerchantCode, withdrawOrder.ID);
                                            }
                                        //}
                                    }
                                }
                            }
                        }

                        // 在 Redis 中标记订单已处理
                        _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间，防止长期锁定
                    }
                }
            }catch (Exception ex)
            {
                NlogLogger.Error("商户代付订单流水异常：" + ex);
                throw;
            }
        }

        /// <summary>
        /// 商户提现
        /// </summary>
        /// <param name="mqDto"></param>
        /// <returns></returns>
        [CapSubscribe(MQSubscribeStaticConsts.MerchantBillAddWithdraws)]
        public async Task MerchantBillAddWithdraws(MerchantWithdrawsMqDto mqDto)
        {
            try
            {
                if (mqDto != null)
                {
                    if (!mqDto.MerchantCode.IsNullOrEmpty() && mqDto.MerchantWithdrawId > 0)
                    {
                        var cacheKey = $"merchant-withdraw:{mqDto.MerchantCode}:{mqDto.MerchantWithdrawId}";
                        var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
                        if (isProcessed)
                        {
                            return; // 直接返回，避免重复处理
                        }

                        var merchantWithdraw = await _merchantWithdrawService.GetFirstAsync(r => r.Id == mqDto.MerchantWithdrawId);
                        if(merchantWithdraw != null)
                        {
                            if (merchantWithdraw.Status == MerchantWithdrawStatusEnum.Pass)
                            {
                                var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(mqDto.MerchantCode, merchantWithdraw.WithDrawNo, MerchantBillTypeEnum.Withdraw);
                                if (info == null)
                                {
                                    MerchantWithdrawMongoEntity merchantWithdrawMongoEntity = new MerchantWithdrawMongoEntity()
                                    {
                                        MerchantCode = merchantWithdraw.MerchantCode,
                                        MerchantId = merchantWithdraw.MerchantId,
                                        WithDrawNo = merchantWithdraw.WithDrawNo,
                                        Money = merchantWithdraw.Money,
                                        ReviewTime = merchantWithdraw.ReviewTime,
                                        CreationTime = merchantWithdraw.CreationTime,
                                    };
                                    var checkOrder = _redisService.GetMerchantMqBillOrder(merchantWithdrawMongoEntity.MerchantCode, merchantWithdrawMongoEntity.WithDrawNo);
                                    if (checkOrder.IsNullOrEmpty())
                                    {
                                        //using (_redisService.GetMerchantBalanceLock(mqDto.MerchantCode)) // avoiding same merchant update balance by multiple part of code
                                        //{
                                            var result = await _merchantBillsHelper.AddRetryMerchantWithdrawBillAsync(mqDto.MerchantCode, merchantWithdrawMongoEntity);
                                            if (result)
                                            {
                                                _redisService.SetMerchantMqBillOrder(merchantWithdrawMongoEntity.MerchantCode, merchantWithdrawMongoEntity.WithDrawNo);
                                            }
                                        //}
                                    }
                                }
                            }
                        }

                        // 在 Redis 中标记订单已处理
                        _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间，防止长期锁定
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("商户提现流水异常：" + ex);
                throw;
            }
        }
    }
}

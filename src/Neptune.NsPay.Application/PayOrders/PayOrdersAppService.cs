using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Authorization.Roles;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Common;
using Neptune.NsPay.Common.Dto;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Dto;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.PayOrders.Exporting;
using Neptune.NsPay.RedisExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune.NsPay.PayOrders
{
    [AbpAuthorize(AppPermissions.Pages_PayOrders)]
    public class PayOrdersAppService : NsPayAppServiceBase, IPayOrdersAppService
    {
        private readonly RoleManager _roleManager;
        private readonly IRepository<PayMent> _payMentRepository;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRedisService _redisService;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly PayOrderExcelJob _payOrderExcelJob;
        private readonly IPayOrdersExcelExporter _payOrdersExcelExporter;
        private readonly IKafkaProducer _kafkaProducer;

        public PayOrdersAppService(
            RoleManager roleManager,
            IRepository<PayMent> payMentRepository,
            IPayOrdersMongoService payOrdersMongoService,
            IBackgroundJobManager backgroundJobManager,
            IRedisService redisService,
            IAppConfigurationAccessor appConfigurationAccessor,
            PayOrderExcelJob payOrderExcelJob,
            IPayOrdersExcelExporter payOrdersExcelExporter,
            IKafkaProducer kafkaProducer)
        {
            _roleManager = roleManager;
            _payMentRepository = payMentRepository;
            _payOrdersMongoService = payOrdersMongoService;
            _backgroundJobManager = backgroundJobManager;
            _redisService = redisService;
            _appConfiguration = appConfigurationAccessor.Configuration;
            _payOrderExcelJob = payOrderExcelJob;
            _payOrdersExcelExporter = payOrdersExcelExporter;
            _kafkaProducer = kafkaProducer;
        }

        public virtual async Task<PagedResultDto<GetPayOrderForViewDto>> GetAll(GetAllPayOrdersInput input)
        {
            var stopwatch = new Stopwatch();
            var totalStopwatch = new Stopwatch();

            totalStopwatch.Start();

            var countryCode = input.UtcTimeFilter switch
            {
                "GMT8+" => CultureTimeHelper.TimeCodeZhCN,
                "GMT7+" => CultureTimeHelper.TimeCodeViVn,
                "GMT4-" => CultureTimeHelper.TimeCodeEST,
                _ => CultureTimeHelper.TimeCodeZhCN
            };

            var user = await GetCurrentUserAsync();
            var merchantDict = user.Merchants.ToDictionary(x => x.Id, x => x.Name);
            var userMerchantIdsList = merchantDict.Keys.ToList();

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
            {
                var merchantDetails = user.Merchants.FirstOrDefault(x => x.MerchantCode == input.MerchantCodeFilter);

                if (merchantDetails != null)
                {
                    userMerchantIdsList = new List<int>() { merchantDetails.Id };
                    input.MerchantCodeFilter = string.Empty;
                }
            }

            // Payment filtering
            var paymentQuery = _payMentRepository.GetAll();

            if (!string.IsNullOrEmpty(input.CardNumberFilter))
                paymentQuery = paymentQuery.Where(x => x.CardNumber.Contains(input.CardNumberFilter));

            if (input.OrderBankFilter is > 0)
                paymentQuery = paymentQuery.Where(x => (int)x.Type == input.OrderBankFilter);

            // Filter USDT
            paymentQuery = paymentQuery.Where(x => !PayMentHelper.GetCryptoList.Contains(x.Type));

            var paymentDict = await paymentQuery.ToDictionaryAsync(x => x.Id, x => x);

            stopwatch.Start();
            var filteredPayOrders = await _payOrdersMongoService.GetAllWithPagination(input, userMerchantIdsList, paymentDict.Keys.ToList());
            stopwatch.Stop();

            Console.WriteLine("Pay order Paging time taken " + stopwatch.ElapsedMilliseconds + "ms");

            if (filteredPayOrders != null)
            {
                var culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

                stopwatch.Restart();

                var results = new List<GetPayOrderForViewDto>();
                foreach (var filteredPayOrder in filteredPayOrders.Items)
                {
                    var payOrderQuery = new GetPayOrderForViewDto
                    {
                        PayOrder = new PayOrderDto
                        {
                            Id = filteredPayOrder.ID,
                            MerchantCode = filteredPayOrder.MerchantCode,
                            OrderNo = filteredPayOrder.OrderNo,
                            TransactionNo = filteredPayOrder.TransactionNo,
                            OrderType = filteredPayOrder.OrderType,
                            OrderStatus = filteredPayOrder.OrderStatus,
                            OrderMoney = filteredPayOrder.OrderMoney.ToString("C0", culInfo),
                            OrderMoneyAmount = filteredPayOrder.OrderMoney,
                            Rate = filteredPayOrder.Rate,
                            FeeMoney = filteredPayOrder.FeeMoney.ToString("C0", culInfo),
                            FeeMoneyAmount = filteredPayOrder.FeeMoney,
                            OrderTime = filteredPayOrder.OrderTime,
                            TransactionTime = CultureTimeHelper.GetCultureTimeInfo(filteredPayOrder.TransactionTime, countryCode),
                            CreationTime = CultureTimeHelper.GetCultureTimeInfo(filteredPayOrder.CreationTime, countryCode),
                            OrderMark = filteredPayOrder.OrderMark,
                            OrderNumber = filteredPayOrder.OrderNumber,
                            PlatformCode = filteredPayOrder.PlatformCode,
                            ScCode = filteredPayOrder.ScCode,
                            ScSeri = filteredPayOrder.ScSeri,
                            ScoreStatus = filteredPayOrder.ScoreStatus,
                            PayTypeStr = filteredPayOrder.PayType.ToString(),
                            PaymentChannel = filteredPayOrder.PaymentChannel,
                            ErrorMsg = filteredPayOrder.ErrorMsg,
                            UserNo = filteredPayOrder.UserNo,
                            Remark = filteredPayOrder.Remark,
                        },
                        MerchantName = merchantDict.ContainsKey(filteredPayOrder.MerchantId) ? merchantDict[filteredPayOrder.MerchantId] : string.Empty,
                        PayMent = ObjectMapper.Map<PayMentDto>(paymentDict.GetValueOrDefault(filteredPayOrder.PayMentId))
                    };

                    results.Add(payOrderQuery);
                }

                stopwatch.Stop();

                Console.WriteLine("Pay order Mapping  time taken " + stopwatch.ElapsedMilliseconds + "ms");

                totalStopwatch.Stop();

                Console.WriteLine("Pay order Total Service  time taken " + totalStopwatch.ElapsedMilliseconds + "ms");

                return new PayOrderPageResultDto<GetPayOrderForViewDto>
                {
                    FeeMoneyTotal = filteredPayOrders.FeeMoneyTotal,
                    OrderMoneyTotal = filteredPayOrders.OrderMoneyTotal,
                    TotalCount = filteredPayOrders.TotalCount,
                    Items = results,
                };
            }

            return null;
        }

        public virtual async Task<PagedResultDto<GetPayOrderForViewDto>> GetAllCrypto(GetAllPayOrdersInput input)
        {
            var stopwatch = new Stopwatch();
            var totalStopwatch = new Stopwatch();

            totalStopwatch.Start();

            var countryCode = input.UtcTimeFilter switch
            {
                "GMT8+" => CultureTimeHelper.TimeCodeZhCN,
                "GMT7+" => CultureTimeHelper.TimeCodeViVn,
                "GMT4-" => CultureTimeHelper.TimeCodeEST,
                _ => CultureTimeHelper.TimeCodeZhCN
            };

            var user = await GetCurrentUserAsync();
            var merchantDict = user.Merchants.ToDictionary(x => x.Id, x => x.Name);
            var userMerchantIdsList = merchantDict.Keys.ToList();

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
            {
                var merchantDetails = user.Merchants.FirstOrDefault(x => x.MerchantCode == input.MerchantCodeFilter);

                if (merchantDetails != null)
                {
                    userMerchantIdsList = new List<int>() { merchantDetails.Id };
                    input.MerchantCodeFilter = string.Empty;
                }
            }

            // Payment filtering
            var paymentQuery = _payMentRepository.GetAll();

            if (input.OrderBankFilter is > 0 && PayMentHelper.GetCryptoList.Any(x => (int)x == input.OrderBankFilter))
                paymentQuery = paymentQuery.Where(x => (int)x.Type == input.OrderBankFilter);
            else
                paymentQuery = paymentQuery.Where(x => PayMentHelper.GetCryptoList.Contains(x.Type));

            var paymentDict = await paymentQuery.ToDictionaryAsync(x => x.Id, x => x);

            stopwatch.Start();
            var filteredPayOrders = await _payOrdersMongoService.GetAllWithPagination(input, userMerchantIdsList, paymentDict.Keys.ToList());
            stopwatch.Stop();

            Console.WriteLine("Pay order Paging time taken " + stopwatch.ElapsedMilliseconds + "ms");

            if (filteredPayOrders != null)
            {
                var culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

                stopwatch.Restart();

                var results = filteredPayOrders.Items.Select(filteredPayOrder => new GetPayOrderForViewDto
                {
                    PayOrder = new PayOrderDto
                    {
                        Id = filteredPayOrder.ID,
                        MerchantCode = filteredPayOrder.MerchantCode,
                        OrderNo = filteredPayOrder.OrderNo,
                        TransactionNo = filteredPayOrder.TransactionNo,
                        OrderType = filteredPayOrder.OrderType,
                        OrderStatus = filteredPayOrder.OrderStatus,
                        OrderMoney = $"{filteredPayOrder.OrderMoney} ₮",
                        OrderMoneyAmount = filteredPayOrder.OrderMoney,
                        Rate = filteredPayOrder.Rate,
                        FeeMoney = $"{filteredPayOrder.FeeMoney + (filteredPayOrder.Rate != 0 ? filteredPayOrder.OrderMoney * filteredPayOrder.Rate : 0)} ₮",
                        FeeMoneyAmount = filteredPayOrder.FeeMoney,
                        OrderTime = filteredPayOrder.OrderTime,
                        TransactionTime = CultureTimeHelper.GetCultureTimeInfo(filteredPayOrder.TransactionTime, countryCode),
                        CreationTime = CultureTimeHelper.GetCultureTimeInfo(filteredPayOrder.CreationTime, countryCode),
                        OrderMark = filteredPayOrder.OrderMark,
                        OrderNumber = filteredPayOrder.OrderNumber,
                        PlatformCode = filteredPayOrder.PlatformCode,
                        ScCode = filteredPayOrder.ScCode,
                        ScSeri = filteredPayOrder.ScSeri,
                        ScoreStatus = filteredPayOrder.ScoreStatus,
                        PayTypeStr = filteredPayOrder.PayType.ToString(),
                        PaymentChannel = filteredPayOrder.PaymentChannel,
                        ErrorMsg = filteredPayOrder.ErrorMsg,
                        UserNo = filteredPayOrder.UserNo,
                        Remark = filteredPayOrder.Remark,
                    },
                    MerchantName = merchantDict.ContainsKey(filteredPayOrder.MerchantId) ? merchantDict[filteredPayOrder.MerchantId] : string.Empty,
                    PayMent = ObjectMapper.Map<PayMentDto>(paymentDict.GetValueOrDefault(filteredPayOrder.PayMentId))
                }).ToList();

                stopwatch.Stop();
                Console.WriteLine("Pay order Mapping time taken " + stopwatch.ElapsedMilliseconds + "ms");

                totalStopwatch.Stop();
                Console.WriteLine("Pay order Total Service time taken " + totalStopwatch.ElapsedMilliseconds + "ms");

                return new PayOrderPageResultDto<GetPayOrderForViewDto>
                {
                    FeeMoneyTotal = filteredPayOrders.FeeMoneyTotal,
                    OrderMoneyTotal = filteredPayOrders.OrderMoneyTotal,
                    TotalCount = filteredPayOrders.TotalCount,
                    Items = results,
                };
            }

            return null;
        }

        public virtual async Task<GetPayOrderForViewDto> GetPayOrderForView(string id)
        {
            var payOrder = await _payOrdersMongoService.GetById(id);

            var output = new GetPayOrderForViewDto { PayOrder = ObjectMapper.Map<PayOrderDto>(payOrder) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrders_Edit)]
        public virtual async Task<GetPayOrderForEditOutput> GetPayOrderForEdit(EntityDto<string> input)
        {
            var payOrder = await _payOrdersMongoService.GetById(input.Id);

            var output = new GetPayOrderForEditOutput { PayOrder = ObjectMapper.Map<CreateOrEditPayOrderDto>(payOrder) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrders_CallBcak)]
        public async Task CallBcak(EntityDto<string> input)
        {
            //修改订单上分状态，等待定时器重新回调数据
            var payOrder = await _payOrdersMongoService.GetById(input.Id);
            if (payOrder.ScoreStatus == PayOrderScoreStatusEnum.Failed)
            {
                payOrder.ScoreStatus = PayOrderScoreStatusEnum.NoScore;
                payOrder.ScoreNumber = 0;
                var user = await GetCurrentUserAsync();
                payOrder.Remark = "用户：" + user.UserName + ",订单号：" + payOrder.OrderNo + ",修改操作回调时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await _payOrdersMongoService.UpdateAsync(payOrder);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrders_EnforceCallBcak)]
        public async Task<ResultViewDto> EnforceCallBcakByBatch(List<string> ListOfPayOrderID)
        {
            // 获取订单列表
            var payOrders = await _payOrdersMongoService.GetBatchNotifcationCheckBox(ListOfPayOrderID);
            ResultViewDto resultViewDto = new ResultViewDto { Code = 200 };

            if (payOrders == null || !payOrders.Any())
            {
                resultViewDto.Code = 404;
                resultViewDto.Message = L("NoOrdersFound");
                return resultViewDto;
            }

            var user = await GetCurrentUserAsync();
            var roleId = await _roleManager.FindByNameAsync("AF1204BAC9004894AC1BB1E4A181CEA1");
            var roles = await GetCurrentUserRoleAsync();
            bool hasPermission = roleId != null && roles.Contains(roleId.Name);

            List<PayOrdersMongoEntity> processableOrders = new List<PayOrdersMongoEntity>();

            foreach (var payOrder in payOrders)
            {
                if (payOrder.OrderStatus != PayOrderOrderStatusEnum.Completed)
                {
                    if (hasPermission || payOrder.OrderMoney <= 10000000)
                    {
                        processableOrders.Add(payOrder);
                    }
                    else
                    {
                        resultViewDto.Code = 255;
                        resultViewDto.Message = L("FailImmediateCallBcak");
                    }
                }
            }

            if (processableOrders.Any())
            {
                await PayOrderUnitOfWorkByBatch(processableOrders, user);
            }

            return resultViewDto;
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrders_EnforceCallBcak)]
        public async Task<ResultViewDto> EnforceCallBcak(EntityDto<string> input)
        {
            //修改订单上分状态，等待定时器重新回调数据
            var payOrder = await _payOrdersMongoService.GetById(input.Id);
            ResultViewDto resultViewDto = new ResultViewDto();
            resultViewDto.Code = 200;
            if (payOrder != null && payOrder.OrderStatus != PayOrderOrderStatusEnum.Completed)
            {
                if (payOrder != null)
                {
                    var user = await GetCurrentUserAsync();
                    var roleId = await _roleManager.FindByNameAsync("AF1204BAC9004894AC1BB1E4A181CEA1");
                    var checkFlag = false;
                    if (roleId != null)
                    {
                        var roles = await GetCurrentUserRoleAsync();
                        var checkRole = roles.FirstOrDefault(r => r == roleId.Name);
                        if (checkRole != null)
                        {
                            checkFlag = true;
                        }
                    }

                    if (checkFlag)
                    {
                        await PayOrderUnitOfWork(payOrder);
                    }
                    else
                    {
                        if (payOrder.OrderMoney <= 10000000)
                        {
                            await PayOrderUnitOfWork(payOrder);
                        }
                        else
                        {
                            resultViewDto.Code = 255;
                            resultViewDto.Message = L("FailImmediateCallBcak");
                        }
                    }
                }
            }
            return resultViewDto;
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrders_AddMerchantBill)]
        public async Task AddMerchantBill(EntityDto<string> input)
        {
            var payOrder = await _payOrdersMongoService.GetById(input.Id);
            if (payOrder != null && payOrder.OrderStatus == PayOrderOrderStatusEnum.Completed)
            {
                var checkOrder = _redisService.GetMerchantBillOrder(payOrder.MerchantCode, payOrder.OrderNumber);
                if (checkOrder.IsNullOrEmpty())
                {
                    _redisService.SetMerchantBillOrder(payOrder.MerchantCode, payOrder.OrderNumber);
                    //var redisMqDto = new PayMerchantRedisMqDto()
                    //{
                    //    PayMqSubType = MQSubscribeStaticConsts.MerchantBillAddBalance,
                    //    MerchantCode = payOrder.MerchantCode,
                    //    PayOrderId = payOrder.ID,
                    //};
                    //_redisService.SetMerchantMqPublish(redisMqDto);
                    var order = new PayOrderPublishDto()
                    {
                        MerchantCode = payOrder.MerchantCode,
                        PayOrderId = payOrder.ID,
                        TriggerDate = DateTime.Now,
                        ProcessId = Guid.NewGuid().ToString()
                    };
                    await _kafkaProducer.ProduceAsync<PayOrderPublishDto>(KafkaTopics.PayOrder, payOrder.ID, order);
                }
            }
        }

        public async Task<IList<GetOrderMerchantViewDto>> GetOrderMerchants()
        {
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants.Select(r => new GetOrderMerchantViewDto
            {
                MerchantCode = r.MerchantCode,
                MerchantName = r.Name
            }).ToList();
            return merchants;
        }

        public bool IsShowMerchantFilter()
        {
            var user = GetCurrentUser();

            if (user.UserType == UserTypeEnum.NsPayAdmin || user.UserType == UserTypeEnum.NsPayKefu)
            {
                return true;
            }
            return false;
        }

        public virtual async Task<string> GetPayOrdersToExcel(GetAllPayOrdersForExcelInput input)
        {
            var user = await GetCurrentUserAsync();
            input.userMerchantIds = user.Merchants.Select(r => r.Id).ToList();

            string inputStr = input.ToJsonString();
            string cacheInput = _redisService.GetPayOrderExcel(user.UserName);
            FileDto file = null;

            var countRecord = await _payOrderExcelJob.GetTotalRecordExcel(input, user.ToUserIdentifier());

            if (inputStr != cacheInput)
            {
                if (countRecord <= 5000)
                {
                    var orderLists = await _payOrderExcelJob.GetPayOrderExportAsync(user.ToUserIdentifier(), input);
                    return ExportPayOrders(orderLists, user);
                }
                else
                {
                    await ScheduleBackgroundJob(input, user, inputStr);
                }
            }

            return string.Empty;
        }

        private string ExportPayOrders(List<GetPayOrderForViewDto> orders, User user)
        {
            var pageNumber = 1;
            var rowCount = 1000000;
            FileDto file = null;

            if (orders == null || orders.Count == 0)
            {
                file = _payOrdersExcelExporter.ExportToFile(new List<GetPayOrderForViewDto>(), pageNumber, user.UserType);
            }
            else
            {
                for (int i = 0; i < orders.Count; i += rowCount)
                {
                    int currentBatchSize = Math.Min(rowCount, orders.Count - i);
                    file = _payOrdersExcelExporter.ExportToFile(orders.GetRange(i, currentBatchSize), pageNumber, user.UserType);
                    pageNumber++;
                }
            }

            return string.Format(NsPayConsts.FileDownloadExcelPath,
                                 file.FileToken,
                                 Uri.EscapeDataString(file.FileType),
                                 Uri.EscapeDataString(file.FileName));
        }

        private async Task ScheduleBackgroundJob(GetAllPayOrdersForExcelInput input, User user, string inputStr)
        {
            var args = new PayOrderExcelJobArgs
            {
                input = input,
                User = user.ToUserIdentifier(),
                UserType = user.UserType
            };

            _redisService.SetPayOrderExcel(user.UserName, inputStr);
            await _backgroundJobManager.EnqueueAsync<PayOrderExcelJob, PayOrderExcelJobArgs>(args);
        }

        private async Task PayOrderUnitOfWork(PayOrdersMongoEntity payOrder)
        {
            var transactionNo = "";
            if (payOrder.TransactionNo.IsNullOrEmpty())
            {
                transactionNo = "QZ" + Guid.NewGuid().ToString("N");
            }
            else
            {
                transactionNo = "QZ" + payOrder.TransactionNo;
            }

            payOrder.OrderType = PayOrderOrderTypeEnum.Enforce;
            payOrder.OrderStatus = PayOrderOrderStatusEnum.Completed;
            payOrder.TransactionNo = transactionNo;
            payOrder.TransactionTime = DateTime.Now;
            payOrder.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(payOrder.TransactionTime);
            payOrder.ScoreStatus = PayOrderScoreStatusEnum.NoScore;
            payOrder.ScoreNumber = 0;
            payOrder.TradeMoney = payOrder.OrderMoney;
            var user = await GetCurrentUserAsync();
            payOrder.Remark = "订单列表，用户：" + user.UserName + ",订单号：" + payOrder.OrderNo + ",操作订单强制成功时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            await _payOrdersMongoService.UpdateAsync(payOrder);

            var checkOrder = _redisService.GetMerchantBillOrder(payOrder.MerchantCode, payOrder.OrderNumber);
            if (string.IsNullOrEmpty(checkOrder))
            {
                _redisService.SetMerchantBillOrder(payOrder.MerchantCode, payOrder.OrderNumber);
                //PayMerchantRedisMqDto redisMqDto = new PayMerchantRedisMqDto()
                //{
                //    PayMqSubType = MQSubscribeStaticConsts.MerchantBillAddBalance,
                //    MerchantCode = payOrder.MerchantCode,
                //    PayOrderId = payOrder.ID,
                //};
                //_redisService.SetMerchantMqPublish(redisMqDto);
                var eventId = Guid.NewGuid().ToString();

                await _kafkaProducer.ProduceAsync<PayOrderCallbackPublishDto>(KafkaTopics.PayOrderCallBack, payOrder.ID, new PayOrderCallbackPublishDto()
                {
                    PayOrderId = payOrder.ID,
                    ProcessId = eventId
                });

                await _kafkaProducer.ProduceAsync<PayOrderPublishDto>(KafkaTopics.PayOrder, payOrder.ID, new PayOrderPublishDto()
                {
                    MerchantCode = payOrder.MerchantCode,
                    PayOrderId = payOrder.ID,
                    TriggerDate = DateTime.Now,
                    ProcessId = eventId
                });
            }
        }

        private async Task PayOrderUnitOfWorkByBatch(List<PayOrdersMongoEntity> payOrders, User user)
        {
            foreach (var payOrder in payOrders)
            {
                var transactionNo = string.IsNullOrEmpty(payOrder.TransactionNo)
                    ? "QZ" + Guid.NewGuid().ToString("N")
                    : "QZ" + payOrder.TransactionNo;

                payOrder.OrderType = PayOrderOrderTypeEnum.Enforce;
                payOrder.OrderStatus = PayOrderOrderStatusEnum.Completed;
                payOrder.TransactionNo = transactionNo;
                payOrder.TransactionTime = DateTime.Now;
                payOrder.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(payOrder.TransactionTime);
                payOrder.ScoreStatus = PayOrderScoreStatusEnum.NoScore;
                payOrder.ScoreNumber = 0;
                payOrder.TradeMoney = payOrder.OrderMoney;
                payOrder.Remark = $"订单列表，用户：{user.UserName}, 订单号：{payOrder.OrderNo}, 操作订单强制成功时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                await _payOrdersMongoService.UpdateAsync(payOrder);
            }

            var eventId = Guid.NewGuid().ToString();

            foreach (var payOrder in payOrders)
            {
                var checkOrder = _redisService.GetMerchantBillOrder(payOrder.MerchantCode, payOrder.OrderNumber);
                if (string.IsNullOrEmpty(checkOrder))
                {
                    _redisService.SetMerchantBillOrder(payOrder.MerchantCode, payOrder.OrderNumber);
                    //PayMerchantRedisMqDto redisMqDto = new PayMerchantRedisMqDto()
                    //{
                    //    PayMqSubType = MQSubscribeStaticConsts.MerchantBillAddBalance,
                    //    MerchantCode = payOrder.MerchantCode,
                    //    PayOrderId = payOrder.ID,
                    //};
                    //_redisService.SetMerchantMqPublish(redisMqDto);

                    await _kafkaProducer.ProduceAsync<PayOrderCallbackPublishDto>(KafkaTopics.PayOrderCallBack, payOrder.ID, new PayOrderCallbackPublishDto()
                    {
                        PayOrderId = payOrder.ID,
                        ProcessId = eventId
                    });

                    await _kafkaProducer.ProduceAsync<PayOrderPublishDto>(KafkaTopics.PayOrder, payOrder.ID, new PayOrderPublishDto()
                    {
                        MerchantCode = payOrder.MerchantCode,
                        PayOrderId = payOrder.ID,
                        TriggerDate = DateTime.Now,
                        ProcessId = eventId
                    });
                }
            }
        }
    }
}
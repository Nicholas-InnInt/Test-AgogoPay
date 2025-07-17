using Abp.Extensions;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.ELKLogExtension;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Web.Api.Helpers;
using Neptune.NsPay.Web.Api.Models;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalOrders;
using Newtonsoft.Json;

namespace Neptune.NsPay.Web.Api.Controllers
{
    public class TransferController : BaseController
    {
        private readonly IRedisService _redisService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IWithdrawalDevicesService _withdrawalDevicesService;
        private readonly MerchantOrderCacheHelper _merchantOrderCacheHelper;
        private readonly string eventItemKey = "merchantOrderEventId";
        private readonly LogOrderService _logOrderService;
        private readonly IKafkaProducer _kafkaProducer;

        public TransferController(IRedisService redisService,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IMerchantFundsMongoService merchantFundsMongoService,
            IWithdrawalDevicesService withdrawalDevicesService,
            MerchantOrderCacheHelper merchantOrderCacheHelper,
            LogOrderService logOrderService,
           IKafkaProducer kafkaProducer)

        {
            _redisService = redisService;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _merchantFundsMongoService = merchantFundsMongoService;
            _withdrawalDevicesService = withdrawalDevicesService;
            _merchantOrderCacheHelper = merchantOrderCacheHelper;
            _logOrderService = logOrderService;
            _kafkaProducer = kafkaProducer;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpPost]
        public async Task<JsonResult> Index([FromBody] PlatformTransferRequest? transferRequest)
        {
            try
            {
                if (Convert.ToInt32(AppSettings.Configuration["WebSite:IsTest"]) == 0 &&
                    AppSettings.Configuration["WebSite:Type"] != "APITransfer")
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "请求参数错误!");
                }

                #region 检查参数

                if (transferRequest is null ||
                    string.IsNullOrEmpty(transferRequest.MerchNo) ||
                    string.IsNullOrEmpty(transferRequest.OrderNo) ||
                    string.IsNullOrEmpty(transferRequest.Sign) ||
                    string.IsNullOrEmpty(transferRequest.BankAccNo) ||
                    string.IsNullOrEmpty(transferRequest.NotifyUrl) ||
                    transferRequest.Money <= 0)
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "请求参数错误");
                }
                NlogLogger.Info("支付请求：" + transferRequest.ToJsonString());

                transferRequest.MerchNo = transferRequest.MerchNo.Trim();
                transferRequest.OrderNo = transferRequest.OrderNo.Trim();
                transferRequest.BankAccNo = transferRequest.BankAccNo.Trim();
                transferRequest.NotifyUrl = transferRequest.NotifyUrl.Trim();
                transferRequest.Sign = transferRequest.Sign.Trim();
                transferRequest.MerchNo = transferRequest.MerchNo.Trim();
                transferRequest.BankName = transferRequest.BankName.Replace(" ", "").ToLower().Trim();

                #endregion 检查参数

                #region 提现银行判断, 小银行直接返回失败

                var unableBankList = _redisService
                    .GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.UnableWithdrawBank)?
                    .Replace(" ", "")
                    .ToLower()
                    .Split(",");
                if (unableBankList is not null && unableBankList.Any(x => x.Trim() == transferRequest.BankName))
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "该银行不支持出款");
                }

                #endregion 提现银行判断, 小银行直接返回失败

                #region 商户检查

                // 检查商户信息
                var merchant = _redisService.GetMerchantKeyValue(transferRequest.MerchNo);
                if (merchant is null)
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "商户号错误");
                }

                if (!RequestSignHelper.CheckTransferSign(transferRequest, merchant.MerchantSecret))
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "签名错误");
                }

                var eventid = _merchantOrderCacheHelper.AddMerchantOrder(merchant.Id, transferRequest.OrderNo, transferRequest.Money);
                if (!string.IsNullOrEmpty(eventid))
                {
                    HttpContext.Items[eventItemKey] = eventid;
                }

                // 商户余额检查
                var funds = await _merchantFundsMongoService.GetFundsByMerchantCode(merchant.MerchantCode);
                var currentSubmitOrderAmount = _merchantOrderCacheHelper.GetProcessOrderAmount(merchant.Id, transferRequest.OrderNo);
                if (funds is null)
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "商户余额不足");
                }

                // locked balance = pending processing balance
                if ((funds.Balance - transferRequest.Money - currentSubmitOrderAmount - funds.FrozenBalance) <= 0)
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "商户余额不足");
                }

                //查询是否有代付中订单，禁止通知批量提交
                //var orderStatusProcess = await _withdrawalOrdersMongoService.GetWithdrawOrderProcess(merchant.MerchantCode);
                //if (orderStatusProcess != null && orderStatusProcess.Count > 0)
                //{
                //    var sumMoney = orderStatusProcess.Sum(r => r.OrderMoney);
                //    if ((funds.Balance - transferRequest.Money - sumMoney- currentSubmitOrderAmount) <= 0)
                //    {
                //        //商户余额不够
                //        return toResponseErrorNew(StatusCodeType.ParameterError, "商户余额不足");
                //    }
                //}

                #endregion 商户检查

                #region 订单检查

                if (await _withdrawalOrdersMongoService.GetWithdrawOrderByOrderNumber(merchant.Id, transferRequest.OrderNo) is not null)
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "请不要重复提交订单");
                }

                #endregion 订单检查

                #region 出款设备

                var internalWithdrawMerchant = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.InternalWithdrawMerchant);
                var merchantCode = internalWithdrawMerchant.Contains(merchant.MerchantCode) ? merchant.MerchantCode : NsPayRedisKeyConst.NsPay;

                var withdrawalDeviceServiceList = _withdrawalDevicesService
                    .GetWhere(r => r.MerchantCode == merchantCode && r.Process == WithdrawalDevicesProcessTypeEnum.Process && r.IsDeleted == false)
                    .Where(device =>
                        (device.MinMoney == 0 && device.MaxMoney == 0) ||
                        (device.MinMoney > 0 && device.MaxMoney == 0 && transferRequest.Money >= device.MinMoney) ||
                        (device.MaxMoney > 0 && device.MinMoney == 0 && transferRequest.Money <= device.MaxMoney) ||
                        (device.MinMoney > 0 && device.MaxMoney > 0 && transferRequest.Money >= device.MinMoney && transferRequest.Money <= device.MaxMoney)
                    )
                    .Select(x => new WithdrawalDeviceInfoModel
                    {
                        Id = x.Id,
                        MerchantCode = x.MerchantCode,
                        Name = x.Name,
                        Phone = x.Phone,
                        CardNumber = x.CardNumber,
                        BankType = x.BankType,
                        BankOtp = x.BankOtp,
                        LoginPassWord = x.LoginPassWord,
                        CardName = x.CardName,
                        Process = x.Process,
                        Status = x.Status,
                        DeviceAdbName = x.DeviceAdbName,
                        MinMoney = x.MinMoney,
                        MaxMoney = x.MaxMoney,
                        Balance = 0,
                        PendingCount = 0,
                        PendingAmount = 0,
                    }).ToList();

                var withdrawBankRule = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.WithdrawBankRule);
                if (!withdrawBankRule.IsNullOrEmpty())
                {
                    var bankModelList = JsonConvert.DeserializeObject<List<NameValueRedisModel>>(withdrawBankRule);
                    if (bankModelList is { Count: > 0 })
                    {
                        var name = transferRequest.BankName.Replace(" ", "").ToLower().Trim();
                        foreach (var bank in bankModelList)
                        {
                            var bankValue = bank.Value.Replace(" ", "").ToLower().Trim();
                            if (!string.IsNullOrEmpty(bankValue) && bankValue.Contains(name))
                            {
                                var banktype = Enum.Parse<WithdrawalDevicesBankTypeEnum>(bank.Name);
                                withdrawalDeviceServiceList = withdrawalDeviceServiceList.Where(r => r.BankType != banktype).ToList();
                            }
                        }
                    }
                }

                if (withdrawalDeviceServiceList.Count <= 0)
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "暂无出款设备，请稍后提交");
                }

                // 获取随机【判断余额，判断金额】
                var deviceIds = withdrawalDeviceServiceList.Select(x => x.Id).ToList();
                var orderCountList = await _withdrawalOrdersMongoService.GetWithdrawOrderCountByDevice(deviceIds);

                foreach (var device in withdrawalDeviceServiceList)
                {
                    var orderCount = orderCountList.Where(r => r.DeviceId == device.Id).ToList();

                    device.Balance = _redisService.GetWitdrawDeviceBalance(device.Id)?.Balance ?? 0;
                    device.PendingCount = orderCount.Count();
                    device.PendingAmount = orderCount.Sum(x => x.OrderMoney);
                }

                if (withdrawalDeviceServiceList.Count(x => x.Balance - x.PendingAmount >= transferRequest.Money) <= 0)
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "暂无出款设备，请稍后提交");
                }

                // 获取设备订单数最少的进行转账
                WithdrawalDeviceInfoModel? selectedDeviceInfo = null;

                // 过滤出没有订单的设备
                var freeDevices = withdrawalDeviceServiceList.Where(x => x.PendingCount == 0).ToList();
                if (freeDevices.Count > 0)
                {
                    var randomIndex = new Random(Guid.NewGuid().GetHashCode()).Next(freeDevices.Count);
                    selectedDeviceInfo = withdrawalDeviceServiceList.FirstOrDefault(r => r.Id == freeDevices[randomIndex].Id);
                }
                else
                {
                    selectedDeviceInfo = withdrawalDeviceServiceList.OrderBy(x => x.PendingCount).FirstOrDefault(x => x.PendingCount > 0);
                }

                if (selectedDeviceInfo is null)
                {
                    return toResponseErrorNew(StatusCodeType.ParameterError, "暂无出款设备，请稍后提交");
                }

                var deviceInfo = new WithdrawalDeviceRedisModel
                {
                    Id = selectedDeviceInfo.Id,
                    MerchantCode = selectedDeviceInfo.MerchantCode,
                    Name = selectedDeviceInfo.Name,
                    Phone = selectedDeviceInfo.Phone,
                    BankType = selectedDeviceInfo.BankType,
                    Status = selectedDeviceInfo.Status,
                    CardName = selectedDeviceInfo.CardName,
                    Process = selectedDeviceInfo.Process,
                    LoginPassWord = selectedDeviceInfo.LoginPassWord,
                    BankOtp = selectedDeviceInfo.BankOtp,
                    DeviceAdbName = selectedDeviceInfo.DeviceAdbName,
                    MinMoney = selectedDeviceInfo.MinMoney,
                    MaxMoney = selectedDeviceInfo.MaxMoney,
                };

                #endregion 出款设备

                #region 订单生成

                var withdrawalOrder = new WithdrawalOrdersMongoEntity();
                withdrawalOrder.MerchantCode = merchant.MerchantCode;
                withdrawalOrder.MerchantId = merchant.Id;
                withdrawalOrder.PlatformCode = merchant.PlatformCode;
                withdrawalOrder.OrderNumber = transferRequest.OrderNo;
                withdrawalOrder.WithdrawNo = OrderHelper.GenerateId();
                withdrawalOrder.OrderMoney = transferRequest.Money;
                withdrawalOrder.Rate = OrderHelper.GetTransferRate(merchant.MerchantRate);
                withdrawalOrder.FeeMoney = OrderHelper.GetFeeMoney(transferRequest.Money, withdrawalOrder.Rate);
                withdrawalOrder.NotifyUrl = transferRequest.NotifyUrl;
                withdrawalOrder.BenAccountNo = transferRequest.BankAccNo;
                withdrawalOrder.BenAccountName = transferRequest.BankAccName;
                withdrawalOrder.BenBankName = transferRequest.BankName.Trim().Replace(" ", "").Replace("-", ""); // 去掉所有空格
                withdrawalOrder.Description = transferRequest.Desc;
                withdrawalOrder.DeviceId = deviceInfo.Id; // 出款设备
                withdrawalOrder.NotifyStatus = WithdrawalNotifyStatusEnum.Wait;
                withdrawalOrder.CreationTime = DateTime.Now;
                withdrawalOrder.OrderType = WithdrawalOrderTypeEnum.NsPayTransfer;
                withdrawalOrder.ReleaseStatus = WithdrawalReleaseStatusEnum.PendingRelease;

                var orderId = await _withdrawalOrdersMongoService.AddAsync(withdrawalOrder);

                #endregion 订单生成

                if (string.IsNullOrEmpty(orderId))
                {
                    return toResponseNew(StatusCodeType.ParameterError, "添加失败，请稍后提交");
                }

                //                   _merchantBillsHelper.UpdateFrozenBalanceWithAttempt(merchant.MerchantCode, transferRequest.Money,eventid  , orderId, 5).FireAndForgetSafeAsync<bool>(
                //ex => { NlogLogger.Error("attemptUpdateLockedBalance Error ", ex); },
                //result => { NlogLogger.Debug("[" + eventid + "] Merchant " + merchant.MerchantCode + " Locked Balance  " + (result ? "Success" : "Failed")); });

                try
                {
                    var transferOrder = new MerchantBalancePublishDto
                    {
                        MerchantCode = merchant.MerchantCode,
                        Type = MerchantBalanceType.Increase,
                        Source = BalanceTriggerSource.WithdrawalOrder,
                        TriggerDate = DateTime.Now,
                        ProcessId = eventid,
                        ReferenceId = orderId,
                        Money = Convert.ToInt32(withdrawalOrder.OrderMoney),
                    };
                    await _kafkaProducer.ProduceAsync(KafkaTopics.MerchantBalance, orderId, transferOrder);
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("Transfer-Kafka推送失败：" + ex.ToString());
                }

                _logOrderService.SubmitLog(new OrderLogModel
                {
                    ActionName = ActionNameList.WithdrawalOrderLockBalance,
                    DeviceId = deviceInfo.Id,
                    LogDate = DateTime.Now,
                    OrderId = orderId,
                    OrderNumber = withdrawalOrder.OrderNumber,
                    User = "Web API",
                    ProceedId = eventid,
                    OrderStatus = withdrawalOrder.OrderStatus.ToString(),
                    ProcessingTimeMs = 0,
                    OrderCreationDate = withdrawalOrder.CreationTime,
                }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + eventid + "] ELK Create Withdrawal Error ", ex); });
                //  如果有设置最大自动代付金额就使用 ，没有就默认5000W，  代付金额超过 不 加入队列手动转账

                var largeOrderAmountStr = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.LargeWithdrawalOrderAmount);
                var actualLargeOrderAmount = !largeOrderAmountStr.IsNullOrEmpty() && int.TryParse(largeOrderAmountStr, out int _value) ? _value : 50000000m;
                if (transferRequest.Money <= actualLargeOrderAmount)
                {
                    //_orderJobService.AddQueryRecipientQueue(new QueryOrderRecipientQueueItems()
                    //{
                    //    MerchantId = withdrawalOrder.MerchantId,
                    //    RecipientAccountName = withdrawalOrder.BenAccountName,
                    //    RecipientAccountNumber = withdrawalOrder.BenAccountNo,
                    //    RecipientBankName = withdrawalOrder.BenBankName,
                    //    SubmitDateTime = DateTime.Now,
                    //    Callback = Results =>
                    //    {
                    //        if (_merchantOrderCacheHelper.AddAutoPayoutLock(orderRedisModel.Id))
                    //        {
                    //            orderRedisModel.IsAccountNameVerified = Results;
                    //            _redisService.SetRPushTransferOrder(deviceInfo.BankType.ToString(), deviceInfo.Phone, orderRedisModel);
                    //            NlogLogger.Warn("Order Added - " + orderRedisModel.Id);
                    //        }
                    //    }
                    //});

                    // _redisService.SetRPushTransferOrder(deviceInfo.BankType.ToString(), deviceInfo.Phone, orderRedisModel);

                    // 加入缓存队列
                    AddAutoPayoutQueueWithDelay(new WithdrawalOrderRedisModel
                    {
                        Id = orderId,
                        MerchantCode = merchant.MerchantCode,
                        OrderNo = withdrawalOrder.OrderNumber,
                        BenBankName = withdrawalOrder.BenBankName,
                        BenAccountNo = withdrawalOrder.BenAccountNo,
                        BenAccountName = withdrawalOrder.BenAccountName,
                        OrderMoney = withdrawalOrder.OrderMoney,
                        MerchantId = withdrawalOrder.MerchantId,
                    }, deviceInfo).FireAndForgetSafeAsync(ex => NlogLogger.Error("Add Auto Payout Device Error", ex));
                }

                return toResponseNew(new WithdrawOrderResult
                {
                    OrderNo = transferRequest.OrderNo,
                    TradeNo = withdrawalOrder.WithdrawNo,
                    Money = transferRequest.Money,
                });
            }
            catch (Exception ex)
            {
                NlogLogger.Error("代付订单异常：" + ex.ToString(), ex);
                return toResponseNew(StatusCodeType.ParameterError, "添加失败，请稍后提交");
            }
        }

        private async Task<bool> AddAutoPayoutQueueWithDelay(WithdrawalOrderRedisModel model, WithdrawalDeviceRedisModel device)
        {
            if (_merchantOrderCacheHelper.AddAutoPayoutLock(model.Id))
            {
                _redisService.SetRPushTransferOrder(device.BankType.ToString(), device.Phone, model);
                NlogLogger.Warn("Order Added (Delay Time Out)" + model.Id);

                return true;
            }

            return false;
        }

        private JsonResult toResponseNew(StatusCodeType statusCode, string retMessage)
        {
            ApiResult response = new ApiResult();
            response.StatusCode = (int)statusCode;
            response.Message = retMessage;

            if (HttpContext.Items.TryGetValue(eventItemKey, out var eventId) && eventId != null)
            {
                _merchantOrderCacheHelper.ReleaseMerchantOrder(eventId.ToString());
            }

            return new JsonResult(response);
        }

        private JsonResult toResponseNew<T>(T data)
        {
            ApiResult<T> response = new ApiResult<T>();
            response.StatusCode = (int)StatusCodeType.Success;
            response.Message = StatusCodeType.Success.GetEnumText();
            response.Data = data;

            if (HttpContext.Items.TryGetValue(eventItemKey, out var eventId) && eventId != null)
            {
                _merchantOrderCacheHelper.ReleaseMerchantOrder(eventId.ToString());
            }

            return new JsonResult(response);
        }

        private JsonResult toResponseErrorNew(StatusCodeType statusCode, string retMessage)
        {
            ApiResult<PayApiResult> response = new ApiResult<PayApiResult>();
            response.StatusCode = (int)statusCode;
            response.Message = retMessage;
            response.Data = null;

            if (HttpContext.Items.TryGetValue(eventItemKey, out var eventId) && eventId != null)
            {
                _merchantOrderCacheHelper.ReleaseMerchantOrder(eventId.ToString());
            }

            return new JsonResult(response);
        }
    }

    internal static class TaskExtension
    {
        public static void FireAndForgetSafeAsync<T>(this Task<T> task, Action<Exception>? errorHandler = null, Action<T>? successHandler = null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    T result = await task;
                    successHandler?.Invoke(result);
                }
                catch (Exception ex)
                {
                    errorHandler?.Invoke(ex);
                    // Optional: log the exception
                }
            });
        }
    }
}
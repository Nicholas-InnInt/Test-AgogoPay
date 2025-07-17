using System.Diagnostics;
using Abp.Collections.Extensions;
using Neptune.NsPay.BillingExtensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.ELKLogExtension;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PlatfromServices.AppServices;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NsPay.Web.RabbitMQApi.Services
{
    //public class TransferOrderConsumer : IConsumer<TransferOrderPublishDto>
    //{
    //    private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
    //    private readonly IMerchantBillsMongoService _merchantBillsMongoService;
    //    private readonly ITransferCallBackService _transferCallBackService;
    //    private readonly IMerchantBillsHelper _merchantBillsHelper;
    //    private readonly IMerchantFundsMongoService _merchantFundsMongoService;
    //    private readonly IRedisService _redisService;
    //    private readonly IMessageBus _bus;
    //    private readonly LogOrderService _logOrderService;
    //    private readonly ReleaseBalanceService _releaseBalanceService;
        

    //    public TransferOrderConsumer(IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
    //        IMerchantBillsMongoService merchantBillsMongoService,
    //        IMerchantBillsHelper merchantBillsHelper,
    //        ITransferCallBackService transferCallBackService,
    //        IMerchantFundsMongoService merchantFundsMongoService,
    //        IRedisService redisService,
    //        IMessageBus bus,
    //        LogOrderService logOrderService,
    //        ReleaseBalanceService releaseBalanceService)
    //    {
    //        _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
    //        _merchantBillsMongoService = merchantBillsMongoService;
    //        _transferCallBackService = transferCallBackService;
    //        _merchantBillsHelper = merchantBillsHelper;
    //        _redisService = redisService;
    //        _merchantFundsMongoService = merchantFundsMongoService;
    //        _bus = bus;
    //        _logOrderService = logOrderService;
    //        _releaseBalanceService = releaseBalanceService;
    //    }
    //    public async Task OnHandle(TransferOrderPublishDto message, CancellationToken cancellationToken)
    //    {
    //        if (message == null)
    //        {
    //            throw new ArgumentNullException(nameof(message), "Message cannot be null.");
    //        }

    //        Stopwatch stopwatch = Stopwatch.StartNew();
    //        var delayMs = (DateTime.Now - message.TriggerDate).TotalMilliseconds;
    //        var orderId = message.WithdrawalOrderId;
    //        var hasReleaseLockedAmount = false;

    //        WithdrawalOrdersMongoEntity? withdrawOrder = null;
    //        bool haveUpdateMerchantBills = false;
    //        try
    //        {


    //            if (message.OrderStatus  == (int)WithdrawalOrderStatusEnum.Success)
    //            {

    //                //添加流水
    //                if (!message.MerchantCode.IsNullOrEmpty() && !message.WithdrawalOrderId.IsNullOrEmpty())
    //                {
    //                    var cacheKey = $"merchant-reduce-balance:{message.MerchantCode}:{message.WithdrawalOrderId}";
    //                    var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
    //                    if (isProcessed)
    //                    {
    //                        return; // 直接返回，避免重复处理
    //                    }

    //                    withdrawOrder = await _withdrawalOrdersMongoService.GetWithdrawOrderByOrderId(message.MerchantCode, message.WithdrawalOrderId);
    //                    if (withdrawOrder != null)
    //                    {
    //                        if (withdrawOrder.OrderStatus == WithdrawalOrderStatusEnum.Success)
    //                        {
    //                            var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(message.MerchantCode, withdrawOrder.OrderNumber, MerchantBillTypeEnum.Withdraw);
    //                            if (info == null)
    //                            {
    //                                var checkOrder = _redisService.GetMerchantMqBillOrder(withdrawOrder.MerchantCode, withdrawOrder.ID);
    //                                if (checkOrder.IsNullOrEmpty())
    //                                {
    //                                    var result = await _merchantBillsHelper.UpdateWithRetryAddWithdrawalOrderBillAsync(message.MerchantCode, withdrawOrder.ID);
    //                                    if (result)
    //                                    {
    //                                        _redisService.SetMerchantMqBillOrder(withdrawOrder.MerchantCode, withdrawOrder.ID);
    //                                        hasReleaseLockedAmount = true;
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                    _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30));
    //                }
    //            }
    //        }
    //        catch(Exception ex)
    //        {
    //            NlogLogger.Error("[" + message.ProcessId + "] 添加代付订单流水异常：" + ex);
    //        }
    //        finally
    //        {
    //            stopwatch.Stop();

    //            _logOrderService.SubmitLog(new OrderLogModel()
    //            {
    //                ActionName = ActionNameList.WithdrawalUpdateMerchantBills,
    //                DeviceId = withdrawOrder?.DeviceId ?? 0,
    //                LogDate = DateTime.Now,
    //                OrderId = orderId,
    //                OrderNumber = withdrawOrder?.OrderNumber ?? string.Empty,
    //                User = "Rabbit MQ API",
    //                ProceedId = message.ProcessId,
    //                OrderStatus = (withdrawOrder?.OrderStatus.ToString()) ?? "NA",
    //                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
    //                OrderCreationDate = withdrawOrder?.CreationTime ?? DateTime.Now,
    //                Desc = "Need Release LockedAmount "+ hasReleaseLockedAmount
    //            }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + message.ProcessId + "] Insert Merchant Bills  Error ", ex); });

    //            NlogLogger.Info(" ["+ message.ProcessId+ "]代付订单异步处理： 用时  " + stopwatch.ElapsedMilliseconds+" ms " + " 延迟 - "+ delayMs+" ms   解冻余额 -"+(hasReleaseLockedAmount) +" 更新流水 - "+(hasReleaseLockedAmount));
    //        }
    //        await Task.CompletedTask;
    //    }
    //}
}

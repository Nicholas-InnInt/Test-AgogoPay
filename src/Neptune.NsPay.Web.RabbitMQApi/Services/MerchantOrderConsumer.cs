using Abp.Extensions;
using MassTransit;
using Neptune.NsPay.BillingExtensions;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.WithdrawalOrders;
using Newtonsoft.Json;
using System.Diagnostics;
using Neptune.NsPay.ELKLogExtension;

namespace Neptune.NsPay.Web.RabbitMQApi.Services
{
    public class MerchantOrderConsumer : IConsumer<TransferOrderPublishDto>,
      IConsumer<PayOrderPublishDto>,
      IConsumer<MerchantWithdrawalPublishDto>,
      IConsumer<MerchantBalancePublishDto>
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IMerchantBillsMongoService _merchantBillsMongoService;
        private readonly IMerchantBillsHelper _merchantBillsHelper;
        private readonly IMerchantBalanceHelper _merchantBalanceHelper;
        private readonly IRedisService _redisService;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IMerchantWithdrawService _merchantWithdrawService;
        private readonly LogOrderService _logOrderService;

        public MerchantOrderConsumer(IPayOrdersMongoService payOrdersMongoService,
IMerchantBillsMongoService merchantBillsMongoService,
IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
IMerchantWithdrawService merchantWithdrawService,
IMerchantBillsHelper merchantBillsHelper,
IMerchantBalanceHelper merchantBalanceHelper,
IRedisService redisService,
LogOrderService logOrderService)
        {
            _payOrdersMongoService = payOrdersMongoService;
            _merchantBillsMongoService = merchantBillsMongoService;
            _merchantBillsHelper = merchantBillsHelper;
            _merchantBalanceHelper = merchantBalanceHelper;
            _redisService = redisService;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _merchantWithdrawService = merchantWithdrawService;
            _logOrderService = logOrderService;
        }

        public async Task Consume(ConsumeContext<PayOrderPublishDto> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Message cannot be null.");
            }
            var message = context.Message;
            NlogLogger.Info("代收流水接受消息：" + JsonConvert.SerializeObject(message));
            var orderId = message.PayOrderId;
            Stopwatch stopwatch = Stopwatch.StartNew();
            var delayMs = (DateTime.Now - message.TriggerDate).TotalMilliseconds;

            PayOrdersMongoEntity? payOrder = null;
            try
            {
                //添加流水
                if (!message.MerchantCode.IsNullOrEmpty() && !message.PayOrderId.IsNullOrEmpty())
                {
                    var cacheKey = $"merchant-add-balance:{message.MerchantCode}:{message.PayOrderId}";
                    var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
                    if (isProcessed)
                    {
                        return; // 直接返回，避免重复处理
                    }

                    payOrder = await _payOrdersMongoService.GetPayOrderByOrderId(message.PayOrderId);
                    if (payOrder != null)
                    {
                        if (payOrder.OrderStatus == PayOrderOrderStatusEnum.Completed)
                        {
                            var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(message.MerchantCode, payOrder.OrderNumber, MerchantBillTypeEnum.OrderIn);
                            if (info == null)
                            {
                                var checkOrder = _redisService.GetMerchantMqBillOrder(payOrder.MerchantCode, payOrder.ID);
                                if (checkOrder.IsNullOrEmpty())
                                {
                                    //using (_redisService.GetMerchantBalanceLock(mqDto.MerchantCode)) // avoiding same merchant update balance by multiple part of code
                                    //{
                                    var result = await _merchantBillsHelper.AddRetryPayOrderBillAsync(message.MerchantCode, payOrder.ID);
                                    //var result = await _merchantBillsHelper.UpdateWithRetryAddPayOrderBillAsync(message.MerchantCode, payOrder.ID);
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
            catch (Exception ex)
            {
                NlogLogger.Error("添加收款订单流水异常：" + ex);
            }
            finally
            {
                stopwatch.Stop();

                _logOrderService.SubmitLog(new OrderLogModel()
                {
                    ActionName = ActionNameList.PayOrderCompleted,
                    LogDate = DateTime.Now,
                    OrderId = orderId,
                    OrderNumber = payOrder?.OrderNumber ?? string.Empty,
                    User = "Rabbit MQ API",
                    ProceedId = message.ProcessId,
                    OrderStatus = (payOrder?.OrderStatus.ToString()) ?? "NA",
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                    OrderCreationDate = payOrder?.CreationTime ?? DateTime.Now,
                    Desc = " Delay " + delayMs + " ms"
                }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + message.ProcessId + "] Insert Merchant Bills  Error ", ex); });

                NlogLogger.Info(" [" + message.ProcessId + "]代收订单异步处理： 用时  " + stopwatch.ElapsedMilliseconds + " ms " + " 延迟 - " + delayMs + " ms ");
            }

            await Task.CompletedTask;
        }

        public async Task Consume(ConsumeContext<TransferOrderPublishDto> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Message cannot be null.");
            }
            var message = context.Message;
            NlogLogger.Info("代付流水接受消息：" + JsonConvert.SerializeObject(message));
            var orderId = message.WithdrawalOrderId;

            Stopwatch stopwatch = Stopwatch.StartNew();
            var delayMs = (DateTime.Now - message.TriggerDate).TotalMilliseconds;
            WithdrawalOrdersMongoEntity? withdrawOrder = null;
            try
            {
                //添加流水
                if (!message.MerchantCode.IsNullOrEmpty() && !message.WithdrawalOrderId.IsNullOrEmpty())
                {
                    var cacheKey = $"merchant-reduce-balance:{message.MerchantCode}:{message.WithdrawalOrderId}";
                    var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
                    if (isProcessed)
                    {
                        return; // 直接返回，避免重复处理
                    }

                    withdrawOrder = await _withdrawalOrdersMongoService.GetWithdrawOrderByOrderId(message.MerchantCode, message.WithdrawalOrderId);
                    if (withdrawOrder != null)
                    {
                        if (withdrawOrder.OrderStatus == WithdrawalOrderStatusEnum.Success)
                        {
                            var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(message.MerchantCode, withdrawOrder.OrderNumber, MerchantBillTypeEnum.Withdraw);
                            if (info == null)
                            {
                                var checkOrder = _redisService.GetMerchantMqBillOrder(withdrawOrder.MerchantCode, withdrawOrder.ID);
                                if (checkOrder.IsNullOrEmpty())
                                {
                                    var result = await _merchantBillsHelper.AddRetryWithdrawalOrderBillAsync(message.MerchantCode, withdrawOrder.ID);
                                    //var result = await _merchantBillsHelper.UpdateWithRetryAddWithdrawalOrderBillAsync(message.MerchantCode, withdrawOrder.ID);
                                    if (result)
                                    {
                                        _redisService.SetMerchantMqBillOrder(withdrawOrder.MerchantCode, withdrawOrder.ID);
                                    }
                                }
                            }
                        }
                    }
                    _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30));
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("添加代付订单流水异常：" + ex);
            }
            finally
            {
                stopwatch.Stop();

                _logOrderService.SubmitLog(new OrderLogModel()
                {
                    ActionName = ActionNameList.WithdrawalUpdateMerchantBills,
                    DeviceId = withdrawOrder?.DeviceId ?? 0,
                    LogDate = DateTime.Now,
                    OrderId = orderId,
                    OrderNumber = withdrawOrder?.OrderNumber ?? string.Empty,
                    User = "Rabbit MQ API",
                    ProceedId = message.ProcessId,
                    OrderStatus = (withdrawOrder?.OrderStatus.ToString()) ?? "NA",
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                    OrderCreationDate = withdrawOrder?.CreationTime ?? DateTime.Now,
                    Desc = " Delay " + delayMs + " ms"
                }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + message.ProcessId + "] Insert Merchant Bills  Error ", ex); });

                NlogLogger.Info(" [" + message.ProcessId + "]代付订单异步处理： 用时  " + stopwatch.ElapsedMilliseconds + " ms " + " 延迟 - " + delayMs + " ms ");
            }
            await Task.CompletedTask;
        }

        public async Task Consume(ConsumeContext<MerchantWithdrawalPublishDto> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Message cannot be null.");
            }
            var message = context.Message;
            NlogLogger.Info("商户提现接受消息：" + JsonConvert.SerializeObject(message));
            var orderId = message.MerchantWithdrawId;
            Stopwatch stopwatch = Stopwatch.StartNew();
            var delayMs = (DateTime.Now - message.TriggerDate).TotalMilliseconds;
            MerchantWithdraw? merchantWithdraw = null;

            try
            {
                //添加流水
                if (!message.MerchantCode.IsNullOrEmpty() && message.MerchantWithdrawId > 0)
                {
                    var cacheKey = $"merchant-withdraw:{message.MerchantCode}:{message.MerchantWithdrawId}";
                    var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
                    if (isProcessed)
                    {
                        return; // 直接返回，避免重复处理
                    }

                    merchantWithdraw = await _merchantWithdrawService.GetFirstAsync(r => r.Id == message.MerchantWithdrawId);
                    if (merchantWithdraw != null)
                    {
                        if (merchantWithdraw.Status == MerchantWithdrawStatusEnum.Pass)
                        {
                            var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(message.MerchantCode, merchantWithdraw.WithDrawNo, MerchantBillTypeEnum.Withdraw);
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
                                    var result = await _merchantBillsHelper.AddRetryMerchantWithdrawBillAsync(message.MerchantCode, merchantWithdrawMongoEntity);
                                    //var result = await _merchantBillsHelper.UpdateWithRetryAddMerchantWithdrawBillAsync(message.MerchantCode, merchantWithdrawMongoEntity);
                                    if (result)
                                    {
                                        _redisService.SetMerchantMqBillOrder(merchantWithdrawMongoEntity.MerchantCode, merchantWithdrawMongoEntity.WithDrawNo);
                                    }
                                }
                            }
                        }
                    }

                    // 在 Redis 中标记订单已处理
                    _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间，防止长期锁定
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("添加商户提现流水异常：" + ex);
            }
            finally
            {
                stopwatch.Stop();

                _logOrderService.SubmitLog(new OrderLogModel()
                {
                    ActionName = ActionNameList.MerchantWithdrawalCompleted,
                    LogDate = DateTime.Now,
                    OrderId = orderId.ToString(),
                    OrderNumber = merchantWithdraw?.WithDrawNo ?? string.Empty,
                    User = "Rabbit MQ API",
                    ProceedId = message.ProcessId,
                    OrderStatus = (merchantWithdraw?.Status.ToString()) ?? "NA",
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                    OrderCreationDate = merchantWithdraw?.CreationTime ?? DateTime.Now,
                    Desc = " Delay " + delayMs + " ms"
                }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + message.ProcessId + "] Insert Merchant Bills  Error ", ex); });

                NlogLogger.Info(" [" + message.ProcessId + "]商户提款异步处理： 用时  " + stopwatch.ElapsedMilliseconds + " ms " + " 延迟 - " + delayMs + " ms ");
            }

            await Task.CompletedTask;
        }

        public async Task Consume(ConsumeContext<MerchantBalancePublishDto> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Message cannot be null.");
            }
            var message = context.Message;
            NlogLogger.Info("冻结余额接受信息：" + JsonConvert.SerializeObject(message));
            if (message.Money <= 0)
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null.");
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            var delayMs = (DateTime.Now - message.TriggerDate).TotalMilliseconds;
            try
            {
                if (message.Type == MerchantBalanceType.Increase)
                {
                    //增加余额
                    await _merchantBalanceHelper.AddMerchantFundsFrozenBalance(message.MerchantCode, message.Money);
                }
                else if (message.Type == MerchantBalanceType.Decrease)
                {
                    //减少余额
                    await _merchantBalanceHelper.UnlockFrozenBalanceAsync(message.MerchantCode, message.Money);
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("冻结余额异常：" + ex);
            }

            finally
            {
                stopwatch.Stop();

                _logOrderService.SubmitLog(new OrderLogModel()
                {
                    ActionName = message.Source == BalanceTriggerSource.MerchantWithdrawal ? ActionNameList.MerchantWithdrawalRelease : ActionNameList.WithdrawalReleaseLockedBalance,
                    LogDate = DateTime.Now,
                    OrderId = message.ReferenceId,
                    OrderNumber = string.Empty,
                    User = "Rabbit MQ API",
                    ProceedId = message.ProcessId,
                    OrderStatus = "NA",
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                    OrderCreationDate = DateTime.Now,
                    Desc = " Delay " + delayMs + " ms"
                }).FireAndForgetSafeAsync(ex => { NlogLogger.Error("[" + message.ProcessId + "] Insert Merchant Bills  Error ", ex); });

                NlogLogger.Info(" [" + message.ProcessId + "]冻结余额异步处理： 用时  " + stopwatch.ElapsedMilliseconds + " ms " + " 延迟 - " + delayMs + " ms ");
            }
            await Task.CompletedTask;
        }
    }
}

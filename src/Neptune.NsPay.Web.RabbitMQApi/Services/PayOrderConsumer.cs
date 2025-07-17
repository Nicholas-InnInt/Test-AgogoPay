using Abp.Collections.Extensions;
using Neptune.NsPay.BillingExtensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;

namespace Neptune.NsPay.Web.RabbitMQApi.Services
{
    //public class PayOrderConsumer: IConsumer<PayOrderPublishDto>
    //{
    //    private readonly IPayOrdersMongoService _payOrdersMongoService;
    //    private readonly IMerchantBillsMongoService _merchantBillsMongoService;
    //    private readonly ICallBackService _callBackService;
    //    private readonly IMerchantBillsHelper _merchantBillsHelper;
    //    private readonly IRedisService _redisService;

    //    public PayOrderConsumer(IPayOrdersMongoService payOrdersMongoService,
    //        IMerchantBillsMongoService merchantBillsMongoService,
    //        IMerchantBillsHelper merchantBillsHelper,
    //        ICallBackService callBackService,
    //        IRedisService redisService)
    //    {
    //        _payOrdersMongoService = payOrdersMongoService;
    //        _merchantBillsMongoService = merchantBillsMongoService;
    //        _callBackService = callBackService;
    //        _merchantBillsHelper = merchantBillsHelper;
    //        _redisService = redisService;
    //    }
    //    public async Task OnHandle(PayOrderPublishDto message, CancellationToken cancellationToken)
    //    {
    //        if (message == null)
    //        {
    //            throw new ArgumentNullException(nameof(message), "Message cannot be null.");
    //        }
    //        var orderId = message.PayOrderId;
    //        try
    //        {
    //            //回调平台
    //            await _callBackService.CallBackPost(orderId);
    //        } catch (Exception ex)
    //        {
    //            NlogLogger.Error("回调入款订单异常：" + ex);
    //        }

    //        try
    //        {
    //            //添加流水
    //            if (!message.MerchantCode.IsNullOrEmpty() && !message.PayOrderId.IsNullOrEmpty())
    //            {
    //                var cacheKey = $"merchant-add-balance:{message.MerchantCode}:{message.PayOrderId}";
    //                var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
    //                if (isProcessed)
    //                {
    //                    return; // 直接返回，避免重复处理
    //                }

    //                var payOrder = await _payOrdersMongoService.GetPayOrderByOrderId(message.PayOrderId);
    //                if (payOrder != null)
    //                {
    //                    if (payOrder.OrderStatus == PayOrderOrderStatusEnum.Completed)
    //                    {
    //                        var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(message.MerchantCode, payOrder.OrderNumber, MerchantBillTypeEnum.OrderIn);
    //                        if (info == null)
    //                        {
    //                            var checkOrder = _redisService.GetMerchantMqBillOrder(payOrder.MerchantCode, payOrder.ID);
    //                            if (checkOrder.IsNullOrEmpty())
    //                            {
    //                                //using (_redisService.GetMerchantBalanceLock(mqDto.MerchantCode)) // avoiding same merchant update balance by multiple part of code
    //                                //{
    //                                var result = await _merchantBillsHelper.UpdateWithRetryAddPayOrderBillAsync(message.MerchantCode, payOrder.ID);
    //                                if (result)
    //                                {
    //                                    _redisService.SetMerchantMqBillOrder(payOrder.MerchantCode, payOrder.ID);
    //                                }
    //                                //}
    //                            }
    //                        }
    //                    }
    //                }

    //                // 在 Redis 中标记订单已处理
    //                _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间，防止长期锁定
    //            }
    //        } catch (Exception ex)
    //        {
    //            NlogLogger.Error("添加收款订单流水异常：" + ex);
    //        }

    //        await Task.CompletedTask;

    //    }
    //}
}

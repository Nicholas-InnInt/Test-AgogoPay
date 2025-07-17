using Neptune.NsPay.BillingExtensions;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Newtonsoft.Json;
using Abp.Extensions;

namespace Neptune.NsPay.Web.RabbitMQApi.Services
{
    //public class MerchantWithdrawalConsumer : IConsumer<MerchantWithdrawalPublishDto>
    //{
    //    private readonly IMerchantWithdrawService _merchantWithdrawService;
    //    private readonly IMerchantBillsMongoService _merchantBillsMongoService;
    //    private readonly ICallBackService _callBackService;
    //    private readonly IMerchantBillsHelper _merchantBillsHelper;
    //    private readonly IRedisService _redisService;

    //    public MerchantWithdrawalConsumer(IMerchantWithdrawService merchantWithdrawService,
    //        IMerchantBillsMongoService merchantBillsMongoService,
    //        IMerchantBillsHelper merchantBillsHelper,
    //        ICallBackService callBackService,
    //        IRedisService redisService)
    //    {
    //        _merchantWithdrawService = merchantWithdrawService;
    //        _merchantBillsMongoService = merchantBillsMongoService;
    //        _callBackService = callBackService;
    //        _merchantBillsHelper = merchantBillsHelper;
    //        _redisService = redisService;
    //    }
    //    public async Task OnHandle(MerchantWithdrawalPublishDto message, CancellationToken cancellationToken)
    //    {
    //        if (message == null)
    //        {
    //            throw new ArgumentNullException(nameof(message), "Message cannot be null.");
    //        }
    //        NlogLogger.Info("商户提现接受消息：" + JsonConvert.SerializeObject(message));
    //        var orderId = message.MerchantWithdrawId;

    //        try
    //        {
    //            //添加流水
    //            if (!message.MerchantCode.IsNullOrEmpty() && message.MerchantWithdrawId > 0)
    //            {
    //                var cacheKey = $"merchant-withdraw:{message.MerchantCode}:{message.MerchantWithdrawId}";
    //                var isProcessed = _redisService.GetFullRedis().Get<bool>(cacheKey);
    //                if (isProcessed)
    //                {
    //                    return; // 直接返回，避免重复处理
    //                }

    //                var merchantWithdraw = await _merchantWithdrawService.GetFirstAsync(r => r.Id == message.MerchantWithdrawId);
    //                if (merchantWithdraw != null)
    //                {
    //                    if (merchantWithdraw.Status == MerchantWithdrawStatusEnum.Pass)
    //                    {
    //                        var info = await _merchantBillsMongoService.GetMerchantBillByOrderNo(message.MerchantCode, merchantWithdraw.WithDrawNo, MerchantBillTypeEnum.Withdraw);
    //                        if (info == null)
    //                        {
    //                            MerchantWithdrawMongoEntity merchantWithdrawMongoEntity = new MerchantWithdrawMongoEntity()
    //                            {
    //                                MerchantCode = merchantWithdraw.MerchantCode,
    //                                MerchantId = merchantWithdraw.MerchantId,
    //                                WithDrawNo = merchantWithdraw.WithDrawNo,
    //                                Money = merchantWithdraw.Money,
    //                                ReviewTime = merchantWithdraw.ReviewTime,
    //                                CreationTime = merchantWithdraw.CreationTime,
    //                            };
    //                            var checkOrder = _redisService.GetMerchantMqBillOrder(merchantWithdrawMongoEntity.MerchantCode, merchantWithdrawMongoEntity.WithDrawNo);
    //                            if (checkOrder.IsNullOrEmpty())
    //                            {
    //                                var result = await _merchantBillsHelper.UpdateWithRetryAddMerchantWithdrawBillAsync(message.MerchantCode, merchantWithdrawMongoEntity);
    //                                if (result)
    //                                {
    //                                    _redisService.SetMerchantMqBillOrder(merchantWithdrawMongoEntity.MerchantCode, merchantWithdrawMongoEntity.WithDrawNo);
    //                                }
    //                            }
    //                        }
    //                    }
    //                }

    //                // 在 Redis 中标记订单已处理
    //                _redisService.GetFullRedis().Set<bool>(cacheKey, true, TimeSpan.FromMinutes(30)); // 设置过期时间，防止长期锁定
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            NlogLogger.Error("添加商户提现流水异常：" + ex);
    //        }

    //        await Task.CompletedTask;
    //    }
    //}
}
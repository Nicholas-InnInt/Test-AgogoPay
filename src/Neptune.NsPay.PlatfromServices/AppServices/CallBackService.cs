using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using RestSharp;
using System.Net;

namespace Neptune.NsPay.PlatfromServices.AppServices
{
    public class CallBackService : ICallBackService
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IRedisService _redisService;

        public CallBackService(IPayOrdersMongoService payOrdersMongoService, IRedisService redisService)
        {
            _payOrdersMongoService = payOrdersMongoService;
            _redisService = redisService;
        }

        public async Task<bool> CallBackPost(string orderId)
        {
            var success = false;

            var payOrder = await _payOrdersMongoService.GetPayOrderByOrderId(orderId);
            if (payOrder != null)
            {
                if (payOrder.ScoreStatus != PayOrderScoreStatusEnum.Completed)
                {
                    if (payOrder.OrderStatus == PayOrderOrderStatusEnum.Completed || payOrder.OrderStatus == PayOrderOrderStatusEnum.Failed || payOrder.OrderStatus == PayOrderOrderStatusEnum.TimeOut)
                    {
                        var notifyurl = payOrder.NotifyUrl;
                        if (!string.IsNullOrEmpty(notifyurl))
                        {
                            string message = "支付失败";
                            int tradeResult = 0;
                            if (payOrder.OrderStatus == PayOrderOrderStatusEnum.Completed)
                            {
                                tradeResult = 1;
                                message = "支付成功";
                            }
                            var merchant = _redisService.GetMerchantKeyValue(payOrder.MerchantCode);
                            IDictionary<string, string> parameters = new Dictionary<string, string>
                            {
                                { "merchNo".ToLower(), payOrder.MerchantCode.ToLower() },
                                { "mchorderNo".ToLower(), payOrder.OrderNo.ToLower() },
                                { "orderNo".ToLower(), payOrder.OrderNumber.ToLower() },
                                { "payType".ToLower(), ToDescription(payOrder.PayType).ToLower() },
                                { "tradeMoney".ToLower(), payOrder.OrderMoney.ToString().ToLower() },
                                { "money".ToLower(), payOrder.OrderMoney.ToString().ToLower() }
                            };
                            var paradic = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
                            string sign = SignHelper.GetSignContent(paradic).ToLower() + "&secret=" + merchant.MerchantSecret.ToLower();
                            sign = MD5Helper.MD5Encrypt32(sign).ToLower();
                            var param = new
                            {
                                tradeResult = tradeResult,
                                merchNo = payOrder.MerchantCode,
                                mchOrderNo = payOrder.OrderNo,
                                payType = ToDescription(payOrder.PayType),
                                tradeMoney = payOrder.OrderMoney.ToString(),
                                orderNo = payOrder.OrderNumber.ToString(),
                                money = payOrder.OrderMoney.ToString(),
                                message = message,
                                sign = sign
                            };
                            try
                            {
                                NlogLogger.Info("回调接口：" + notifyurl + ",回调参数：" + param.ToJsonString());

                                var client = new RestClient(notifyurl);
                                var request = new RestRequest().AddParameter("application/json", param.ToJsonString(), ParameterType.RequestBody);

                                var response = await client.ExecutePostAsync(request);

                                NlogLogger.Info("平台返回：" + response.Content);

                                if (response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
                                {
                                    payOrder.ScoreNumber = payOrder.ScoreNumber + 1;

                                    if (response.Content.Trim().ToLower() == "success")
                                    {
                                        payOrder.ScoreStatus = PayOrderScoreStatusEnum.Completed;
                                        success = true;
                                    }
                                    else
                                    {
                                        payOrder.ScoreStatus = PayOrderScoreStatusEnum.Failed;
                                    }

                                    //更新订单表，修改订单上分状态
                                    await _payOrdersMongoService.UpdateScoreStatus(payOrder);
                                }
                            }
                            catch (Exception ex)
                            {
                                NlogLogger.Error($"回调异常: {ex.Message}", ex);

                                payOrder.ScoreNumber = payOrder.ScoreNumber + 1;
                                payOrder.ScoreStatus = PayOrderScoreStatusEnum.Failed;

                                await _payOrdersMongoService.UpdateScoreStatus(payOrder);
                            }
                        }
                    }
                }
            }
            return success;
        }

        public string ToDescription(PayMentTypeEnum payMent)
        {
            switch (payMent)
            {
                case PayMentTypeEnum.BusinessMbBank:
                    return "smb";
                case PayMentTypeEnum.MBBank:
                    return "smb";
                //case PayMentTypeEnum.TPBank:
                //    return "stpb";
                case PayMentTypeEnum.ACBBank:
                    return "sacb";
                case PayMentTypeEnum.VietcomBank:
                    return "svcb";
                case PayMentTypeEnum.VietinBank:
                    return "svtb";
                case PayMentTypeEnum.BusinessVtbBank:
                    return "svtb";
                case PayMentTypeEnum.BidvBank:
                    return "sbidv";
                case PayMentTypeEnum.BusinessTcbBank:
                    return "stcb";
                case PayMentTypeEnum.TechcomBank:
                    return "stcb";
                case PayMentTypeEnum.PVcomBank:
                    return "spvc";
                case PayMentTypeEnum.MoMoPay:
                    return "momopay";
                case PayMentTypeEnum.ZaloPay:
                    return "zalopay";
                //case PayMentTypeEnum.ViettelPay:
                //    return "viettelpay";
                case PayMentTypeEnum.ScratchCards:
                    return "sc";
                case PayMentTypeEnum.MsbBank:
                    return "smsb";
                case PayMentTypeEnum.SeaBank:
                    return "ssea";
                case PayMentTypeEnum.BvBank:
                    return "sbv";
                case PayMentTypeEnum.NamaBank:
                    return "snama";
                case PayMentTypeEnum.TPBank:
                    return "stp";
                case PayMentTypeEnum.VPBBank:
                    return "svpb";
                case PayMentTypeEnum.OCBBank:
                    return "socb";
                case PayMentTypeEnum.EXIMBank:
                    return "sexim";
                case PayMentTypeEnum.NCBBank:
                    return "sncb";
                case PayMentTypeEnum.HDBank:
                    return "shdb";
                case PayMentTypeEnum.LPBank:
                    return "slpb";
                case PayMentTypeEnum.PGBank:
                    return "spgb";
                case PayMentTypeEnum.VietBank:
                    return "svbb";
                case PayMentTypeEnum.BacaBank:
                    return "sbab";
                default:
                    return "";
            }
        }
    }
}
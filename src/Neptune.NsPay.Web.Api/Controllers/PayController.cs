using Abp.Extensions;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.BankInfo;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.HttpExtensions.PayOnline;
using Neptune.NsPay.HttpExtensions.PayOnline.Models;
using Neptune.NsPay.HttpExtensions.ScratchCard;
using Neptune.NsPay.HttpExtensions.ScratchCard.Models;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Utils;
using Neptune.NsPay.VietQR;
using Neptune.NsPay.Web.Api.Helpers;
using Neptune.NsPay.Web.Api.Models;
using Neptune.NsPay.Web.Api.Services.Interfaces;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Neptune.NsPay.Web.Api.Controllers
{
    public class PayController : BaseController
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayMentManageService _payMentManageService;
        private readonly IScDoiTheCaoHelper _scDoiTheCaoHelper;
        private readonly IRedisService _redisService;
        private readonly ICallBackService _callBackService;
        private readonly IBinaryObjectManagerService _binaryObjectManager;
        private readonly IPayOnlineHelper _payOnlineHelper;
        private readonly IKafkaProducer _kafkaProducer;

        public PayController(
            IRedisService redisService,
            IPayOrdersMongoService payOrdersMongoService,
            IPayMentManageService payMentManageService,
            IScDoiTheCaoHelper scDoiTheCaoHelper,
            ICallBackService callBackService,
            IBinaryObjectManagerService binaryObjectManager,
            IPayOnlineHelper payOnlineHelper,
            IKafkaProducer kafkaProducer)
        {
            _redisService = redisService;
            _payOrdersMongoService = payOrdersMongoService;
            _payMentManageService = payMentManageService;
            _scDoiTheCaoHelper = scDoiTheCaoHelper;
            _callBackService = callBackService;
            _binaryObjectManager = binaryObjectManager;
            _payOnlineHelper = payOnlineHelper;
            _kafkaProducer = kafkaProducer;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] PlatformPayRequest payRequest = null)
        {
            try
            {
                var isTest = Convert.ToInt32(AppSettings.Configuration["WebSite:IsTest"]);
                if (isTest == 0)
                {
                    string siteType = AppSettings.Configuration["WebSite:Type"];
                    if (siteType != "API")
                    {
                        return toResponseError(StatusCodeType.ParameterError, "请求参数错误!");
                    }
                }

                #region 检查提交信息

                if (payRequest.MerchNo.IsNullOrEmpty() || payRequest.OrderNo.IsNullOrEmpty() || payRequest.Sign.IsNullOrEmpty()
                    || payRequest.PayType.IsNullOrEmpty() || payRequest.Money <= 0 || payRequest.NotifyUrl.IsNullOrEmpty())
                {
                    return toResponseError(StatusCodeType.ParameterError, "请求参数错误");
                }
                NlogLogger.Info("支付请求：" + payRequest.ToJsonString());

                //userid和userno如果没有默认空值
                payRequest.MerchNo = payRequest.MerchNo.Trim();
                payRequest.OrderNo = payRequest.OrderNo.Trim();
                payRequest.PayType = payRequest.PayType.Trim();
                payRequest.UserId = payRequest.UserId.IsNullOrEmpty() ? "" : payRequest.UserId.Trim();
                payRequest.UserNo = payRequest.UserNo.IsNullOrEmpty() ? "" : payRequest.UserNo.Trim();
                payRequest.NotifyUrl = payRequest.NotifyUrl.Trim();
                payRequest.Sign = payRequest.Sign.Trim();
                if (payRequest.UserNo.IsNullOrEmpty())
                {
                    payRequest.UserNo = payRequest.UserId;
                }

                PayApiTypeEnum requestPayType = payRequest.PayType.ParseEnum<PayApiTypeEnum>();
                if (requestPayType == PayApiTypeEnum.NoPay)
                {
                    return toResponseError(StatusCodeType.ParameterError, "不支持该支付方式");
                }

                #endregion 检查提交信息

                #region 检查商户信息

                var merchant = _redisService.GetMerchantKeyValue(payRequest.MerchNo);
                if (merchant == null)
                {
                    return toResponseError(StatusCodeType.ParameterError, "商户号错误");
                }

                if (!RequestSignHelper.CheckSign(payRequest, merchant.MerchantSecret))
                {
                    return toResponseError(StatusCodeType.ParameterError, "签名错误");
                }

                var payGroupId = merchant.PayGroupId;
                if (payGroupId <= 0)
                {
                    return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");
                }

                #endregion 检查商户信息

                #region 检查订单信息

                var checkPayOrder = await _payOrdersMongoService.GetPayOrderByOrderNumber(merchant.MerchantCode, payRequest.OrderNo);
                if (checkPayOrder != null)
                {
                    return toResponseError(StatusCodeType.ParameterError, "请不要重复提交订单");
                }

                #endregion 检查订单信息

                #region 检查系统配置

                var maxDepositAmount = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.MaxDepositAmount);
                if (maxDepositAmount != null && int.TryParse(maxDepositAmount, out var _value))
                {
                    if (payRequest.Money > _value)
                    {
                        return toResponseError(StatusCodeType.ParameterError, "订单金额超过最大额度");
                    }
                }

                #endregion 检查系统配置

                #region 检查商户支付信息

                var paymentId = 0;
                var orderPayType = PayMentTypeEnum.NoPay;
                var rate = (decimal)0.00;

                var paygroup = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString());
                if (paygroup is null) return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");

                if (requestPayType == PayApiTypeEnum.Sc)
                {
                    var scPayMent = paygroup.PayMents.Where(r => r.Type == PayMentTypeEnum.ScratchCards).ToList();
                    if (scPayMent == null || !scPayMent.Any())
                    {
                        return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");
                    }
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var payment = scPayMent[rnd.Next(scPayMent.Count())];
                    paymentId = payment.Id;
                    orderPayType = payment.Type;
                }
                else if (requestPayType == PayApiTypeEnum.MoMoPay)
                {
                    var scPayMent = paygroup.PayMents.Where(r => r.Type != PayMentTypeEnum.ScratchCards && r.UseStatus == true && r.UseMoMo == true).ToList();
                    if (merchant.MerchantType is MerchantTypeEnum.External)
                    {
                        var onlinePayMent = paygroup.PayMents.Where(r => r.Type == PayMentTypeEnum.MoMoPay && r.MoMoRate > 0).ToList();
                        scPayMent.AddRange(onlinePayMent);
                    }

                    if (scPayMent == null || !scPayMent.Any())
                    {
                        return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");
                    }

                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var payment = scPayMent[rnd.Next(scPayMent.Count())];
                    paymentId = payment.Id;
                    orderPayType = payment.Type;
                }
                else if (requestPayType == PayApiTypeEnum.ZaloPay)
                {
                    var sePayMent = paygroup.PayMents.Where(r => r.Type == PayMentTypeEnum.ZaloPay && r.ZaloRate > 0).ToList();
                    if (sePayMent == null || !sePayMent.Any())
                    {
                        return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");
                    }

                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var payment = sePayMent[rnd.Next(sePayMent.Count())];
                    paymentId = payment.Id;
                    orderPayType = payment.Type;
                    rate = payment.ZaloRate;
                }
                else if (requestPayType == PayApiTypeEnum.ViettelPay)
                {
                    var fyPayMent = paygroup.PayMents.Where(r => r.Type == PayMentTypeEnum.ViettelPay && r.VittelPayRate > 0).ToList();
                    if (fyPayMent == null || !fyPayMent.Any())
                    {
                        return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");
                    }
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var payment = fyPayMent[rnd.Next(fyPayMent.Count())];
                    paymentId = payment.Id;
                    orderPayType = payment.Type;
                    rate = payment.VittelPayRate;
                }
                else
                {
                    var payments = paygroup.PayMents
                        .Where(x => !PayMentHelper.GetCryptoList.Contains(x.Type))
                        .Where(r => r.UseStatus == true && r.ShowStatus == PayMentStatusEnum.Show)
                        .ToList();

                    var payList = await _payMentManageService.CheckBankPayMents(payments, payRequest.Money);
                    if (payList is not { Count: > 0 })
                        return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");

                    // Take at most 10 lowest balance and random
                    var takeTopLowestBalance = payList.OrderBy(r => _redisService.GetBalanceByPaymentId(r.Id)?.Balance2).Take(10).ToList();
                    var rnd = new Random(Guid.NewGuid().GetHashCode());
                    var selectedPayment = takeTopLowestBalance[rnd.Next(takeTopLowestBalance.Count)];
                    paymentId = selectedPayment.Id;
                    orderPayType = selectedPayment.Type;
                }

                #region 订单生成

                //添加商户自定义备注
                var bankOrderMark = !string.IsNullOrEmpty(merchant.MerchantSetting?.OrderBankRemark) ?
                    merchant.MerchantSetting.OrderBankRemark :
                    _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.OrderBankRemark);

                if (requestPayType != PayApiTypeEnum.ZaloPay && requestPayType != PayApiTypeEnum.ViettelPay)
                {
                    rate = OrderHelper.GetRate(merchant.MerchantRate, orderPayType);
                }

                var feeMoney = OrderHelper.GetFeeMoney(payRequest.Money, rate);
                var payOrdersMongoEntity = new PayOrdersMongoEntity
                {
                    MerchantCode = merchant.MerchantCode,
                    MerchantId = merchant.Id,
                    OrderNumber = payRequest.OrderNo,
                    OrderNo = OrderHelper.GenerateId(),
                    OrderType = PayOrderOrderTypeEnum.Receive,
                    OrderStatus = PayOrderOrderStatusEnum.NotPaid,
                    OrderMoney = payRequest.Money,
                    Rate = rate,
                    FeeMoney = feeMoney,
                    OrderMark = OrderHelper.GenerateMark(bankOrderMark),
                    PlatformCode = merchant.PlatformCode,
                    PayMentId = paymentId,
                    UserId = payRequest.UserId,
                    UserNo = payRequest.UserNo,
                    NotifyUrl = payRequest.NotifyUrl,
                    PayType = orderPayType,
                    ScoreStatus = PayOrderScoreStatusEnum.NoScore,
                    ScoreNumber = 0,
                    PaymentChannel = payRequest.PayType.ParseEnum<PaymentChannelEnum>(),
                    MerchantType = merchant.MerchantType,
                };

                var orderId = await _payOrdersMongoService.AddAsync(payOrdersMongoEntity);

                #endregion 订单生成

                var usingNewUI = !string.IsNullOrEmpty(AppSettings.Configuration["WebSite:PaymentUrl"]);
                string siteUrl = usingNewUI ? AppSettings.Configuration["WebSite:PaymentUrl"] : AppSettings.Configuration["WebSite:BaseUrl"]; //AppSettings.Configuration["WebSite:BaseUrl"];
                string localIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).First().ToString();

                if (siteUrl.IsNullOrEmpty())
                {
                    return toResponseError(StatusCodeType.ParameterError, "地址错误");
                }
                if (requestPayType == PayApiTypeEnum.Sc)
                {
                    PayApiResult payApiResult = new PayApiResult()
                    {
                        OrderNo = payRequest.OrderNo,
                        PayPageurl = usingNewUI ? (siteUrl + "ScratchCard?orderId=" + orderId) : (siteUrl + "Pay/ScratchCard?orderid=" + orderId)
                    };
                    return toResponse<PayApiResult>(payApiResult);
                }
                else if (requestPayType == PayApiTypeEnum.MoMoPay)
                {
                    var payMent = _redisService.GetPayMentInfoById(paymentId);
                    if (payMent.UseMoMo == true)
                    {
                        //标识银行卡使用momo
                        PayApiResult payApiResult = new PayApiResult()
                        {
                            OrderNo = payRequest.OrderNo,
                            PayPageurl = usingNewUI ? (siteUrl + "Momo?orderId=" + orderId) : (siteUrl + "Pay/BankPay?orderid=" + orderId)
                        };
                        return toResponse<PayApiResult>(payApiResult);
                    }
                    else
                    {
                        var payUrl = "";
                        //使用三方MOMO
                        if (payMent.CompanyType == PayMentCompanyTypeEnum.FengYangPay)
                        {
                            payUrl = await FyPay(payMent, payRequest, siteUrl, localIp);
                        }
                        if (payMent.CompanyType == PayMentCompanyTypeEnum.SixEightPay)
                        {
                            payUrl = await SePay(payMent, payRequest, siteUrl, localIp);
                        }
                        PayApiResult payApiResult = new PayApiResult()
                        {
                            OrderNo = payRequest.OrderNo,
                            PayPageurl = payUrl
                        };
                        return toResponse<PayApiResult>(payApiResult);
                    }
                }
                else if (requestPayType == PayApiTypeEnum.ZaloPay)
                {
                    var payMent = _redisService.GetPayMentInfoById(paymentId);
                    var payUrl = "";
                    if (payMent.CompanyType == PayMentCompanyTypeEnum.FengYangPay)
                    {
                        payUrl = await FyPay(payMent, payRequest, siteUrl, localIp);
                    }
                    if (payMent.CompanyType == PayMentCompanyTypeEnum.SixEightPay)
                    {
                        payUrl = await SePay(payMent, payRequest, siteUrl, localIp);
                    }

                    PayApiResult payApiResult = new PayApiResult()
                    {
                        OrderNo = payRequest.OrderNo,
                        PayPageurl = payUrl
                    };
                    return toResponse<PayApiResult>(payApiResult);
                }
                else if (requestPayType == PayApiTypeEnum.ViettelPay)
                {
                    var payMent = _redisService.GetPayMentInfoById(paymentId);
                    var payUrl = "";
                    if (payMent.CompanyType == PayMentCompanyTypeEnum.FengYangPay)
                    {
                        payUrl = await FyPay(payMent, payRequest, siteUrl, localIp);
                    }
                    if (payMent.CompanyType == PayMentCompanyTypeEnum.SixEightPay)
                    {
                        payUrl = await SePay(payMent, payRequest, siteUrl, localIp);
                    }

                    PayApiResult payApiResult = new PayApiResult()
                    {
                        OrderNo = payRequest.OrderNo,
                        PayPageurl = payUrl
                    };
                    return toResponse<PayApiResult>(payApiResult);
                }
                else
                {
                    PayApiResult payApiResult = new PayApiResult()
                    {
                        OrderNo = payRequest.OrderNo,
                        PayPageurl = usingNewUI ? (siteUrl + "Bank?orderId=" + orderId) : (siteUrl + "Pay/BankPay?orderid=" + orderId)
                    };
                    return toResponse<PayApiResult>(payApiResult);
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("PayIndex请求订单异常:" + ex.ToString());
                return toResponse(StatusCodeType.Error, "请求异常");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CryptoPay([FromBody] PlatformPayRequest payRequest)
        {
            try
            {
                if (Convert.ToInt32(AppSettings.Configuration["WebSite:IsTest"]) == 0 && AppSettings.Configuration["WebSite:Type"] != "API")
                {
                    return toResponseError(StatusCodeType.ParameterError, "请求参数错误!");
                }

                #region 检查提交信息

                if (payRequest.MerchNo.IsNullOrEmpty() ||
                    payRequest.OrderNo.IsNullOrEmpty() ||
                    payRequest.Sign.IsNullOrEmpty() ||
                    payRequest.PayType.IsNullOrEmpty() ||
                    payRequest.Money <= 0 ||
                    payRequest.NotifyUrl.IsNullOrEmpty())
                {
                    return toResponseError(StatusCodeType.ParameterError, "请求参数错误");
                }
                NlogLogger.Info("支付请求：" + payRequest.ToJsonString());

                //userid和userno如果没有默认空值
                payRequest.MerchNo = payRequest.MerchNo.Trim();
                payRequest.OrderNo = payRequest.OrderNo.Trim();
                payRequest.PayType = payRequest.PayType.Trim();
                payRequest.UserId = payRequest.UserId.IsNullOrEmpty() ? "" : payRequest.UserId.Trim();
                payRequest.UserNo = payRequest.UserNo.IsNullOrEmpty() ? "" : payRequest.UserNo.Trim();
                payRequest.NotifyUrl = payRequest.NotifyUrl.Trim();
                payRequest.Sign = payRequest.Sign.Trim();
                if (payRequest.UserNo.IsNullOrEmpty())
                {
                    payRequest.UserNo = payRequest.UserId;
                }

                var orderPayType = payRequest.PayType.ParseEnum<PayApiTypeEnum>() switch
                {
                    PayApiTypeEnum.USDT_TRC20 => PayMentTypeEnum.USDT_TRC20,
                    PayApiTypeEnum.USDT_ERC20 => PayMentTypeEnum.USDT_ERC20,
                    _ => PayMentTypeEnum.NoPay
                };

                if (orderPayType is PayMentTypeEnum.NoPay)
                    return toResponseError(StatusCodeType.ParameterError, "不支持该支付方式");

                #endregion 检查提交信息

                #region 检查商户信息

                var merchant = _redisService.GetMerchantKeyValue(payRequest.MerchNo);
                if (merchant == null)
                {
                    return toResponseError(StatusCodeType.ParameterError, "商户号错误");
                }

                if (!RequestSignHelper.CheckSign(payRequest, merchant.MerchantSecret))
                {
                    return toResponseError(StatusCodeType.ParameterError, "签名错误");
                }

                if (merchant.PayGroupId <= 0)
                {
                    return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");
                }

                #endregion 检查商户信息

                #region 检查订单信息

                if (await _payOrdersMongoService.GetPayOrderByOrderNumber(merchant.MerchantCode, payRequest.OrderNo) != null)
                {
                    return toResponseError(StatusCodeType.ParameterError, "请不要重复提交订单");
                }

                #endregion 检查订单信息

                #region 检查系统配置

                var maxDepositAmount = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.MaxDepositAmount);
                if (maxDepositAmount != null && int.TryParse(maxDepositAmount, out var _value) && payRequest.Money > _value)
                {
                    return toResponseError(StatusCodeType.ParameterError, "订单金额超过最大额度");
                }

                #endregion 检查系统配置

                #region 订单生成

                var paymentList = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString())?
                    .PayMents
                    .Where(r => r.UseStatus == true && r.ShowStatus == PayMentStatusEnum.Show)
                    .Where(r => r.Type == orderPayType)
                    .ToList();
                if (paymentList is not { Count: > 0 })
                    return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");

                var payList = await _payMentManageService.CheckBankPayMents(paymentList, payRequest.Money);
                if (payList is not { Count: > 0 })
                    return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");

                // Random get 1 wallet
                var random = new Random();
                var selectedPayment = payList[random.Next(payList.Count)];

                var randomConversionRate = Math.Truncate(UtilsHelper.GetRandomDecimal(selectedPayment.CryptoMinConversionRate ?? 0, selectedPayment.CryptoMaxConversionRate ?? 0));
                var convertedOrderMoney = randomConversionRate != 0 ? (payRequest.Money / randomConversionRate) : payRequest.Money;
                var rateFees = merchant.MerchantRate.USDTRateFees is > 0 ? (merchant.MerchantRate.USDTRateFees / 100) : 0;

                var payOrdersMongoEntity = new PayOrdersMongoEntity
                {
                    MerchantCode = merchant.MerchantCode,
                    MerchantId = merchant.Id,
                    OrderNumber = payRequest.OrderNo,
                    OrderNo = OrderHelper.GenerateId(),
                    OrderType = PayOrderOrderTypeEnum.Receive,
                    OrderStatus = PayOrderOrderStatusEnum.NotPaid,
                    OrderMoney = Math.Round(convertedOrderMoney, 2),
                    UnproccessMoney = payRequest.Money,
                    ProcessedMoney = convertedOrderMoney,
                    ConversionRate = randomConversionRate,
                    Rate = orderPayType is PayMentTypeEnum.USDT_TRC20 or PayMentTypeEnum.USDT_ERC20 ? rateFees : default,
                    FeeMoney = orderPayType is PayMentTypeEnum.USDT_TRC20 or PayMentTypeEnum.USDT_ERC20 ? merchant.MerchantRate.USDTFixedFees : default,
                    OrderMark = "",
                    PlatformCode = merchant.PlatformCode,
                    PayMentId = selectedPayment.Id,
                    UserId = payRequest.UserId,
                    UserNo = payRequest.UserNo,
                    NotifyUrl = payRequest.NotifyUrl,
                    PayType = orderPayType,
                    ScoreStatus = PayOrderScoreStatusEnum.NoScore,
                    ScoreNumber = 0,
                    PaymentChannel = payRequest.PayType.ParseEnum<PaymentChannelEnum>(),
                    MerchantType = merchant.MerchantType,
                };

                var orderId = await _payOrdersMongoService.AddAsync(payOrdersMongoEntity);

                #endregion 订单生成

                var paymentUrl = AppSettings.Configuration["WebSite:PaymentUrl"];

                return toResponse(new PayApiResult
                {
                    OrderNo = payRequest.OrderNo,
                    PayPageurl = $"{paymentUrl}Crypto?orderId={orderId}"
                });
            }
            catch (Exception ex)
            {
                NlogLogger.Error($"PayIndex 请求订单异常: {ex.Message}", ex);
                return toResponse(StatusCodeType.Error, "请求异常");
            }
        }

        public async Task<string> FyPay(PayMentRedisModel payMent, PlatformPayRequest payRequest, string? siteUrl, string localIp)
        {
            var channel = "";
            if (payMent.Type == PayMentTypeEnum.MoMoPay)
                channel = "923";
            if (payMent.Type == PayMentTypeEnum.ZaloPay)
                channel = "921";
            if (payMent.Type == PayMentTypeEnum.ViettelPay)
                channel = "925";

            var fyPayRequest = new FyPayRequest()
            {
                Amount = payRequest.Money,
                Channel = channel,
                Notify_url = siteUrl + "/FyCallBack/FyPay",
                Orderid = payRequest.OrderNo,
                Return_url = "",
                Timestamp = TimeHelper.GetUnixTimeStamp(DateTime.Now).ToString(),
                Uid = payMent.CompanyKey,
                UserIp = localIp,
                Key = payMent.CompanySecret,
                Custom = ""
            };
            var reponse = "";
            var result = await _payOnlineHelper.FyPay(payMent.Gateway, fyPayRequest);
            if (result != null)
                reponse = result.result.payurl;

            return reponse;
        }

        public async Task<string> SePay(PayMentRedisModel payMent, PlatformPayRequest payRequest, string? siteUrl, string localIp)
        {
            var sePayRequest = new SePayRequest()
            {
                memberId = payMent.CompanyKey,
                orderNumber = payRequest.OrderNo,
                amount = payRequest.Money,
                callBackUrl = siteUrl + "/SeCallBack/SePay",
                payType = payMent.Type.ToInt().ToString(),
                playUserIp = localIp,
                Key = payMent.CompanySecret,
            };
            var reponse = "";
            var result = await _payOnlineHelper.SePay(payMent.Gateway, sePayRequest);
            if (result != null)
                reponse = result.payUrl;

            return reponse;
        }

        [HttpPost]
        public async Task<IActionResult> SCPAY([FromBody] PlatformSCPayRequest payRequest = null)
        {
            try
            {
                string siteType = AppSettings.Configuration["WebSite:Type"];
                if (siteType != "API")
                {
                    return toResponseError(StatusCodeType.ParameterError, "请求参数错误!");
                }

                #region 检查提交信息

                if (payRequest.MerchNo.IsNullOrEmpty() || payRequest.OrderNo.IsNullOrEmpty() || payRequest.Sign.IsNullOrEmpty()
                    || payRequest.PayType.IsNullOrEmpty() || payRequest.Money <= 0 || payRequest.NotifyUrl.IsNullOrEmpty()
                    || payRequest.Code.IsNullOrEmpty() || payRequest.Seri.IsNullOrEmpty())
                {
                    return toResponseError(StatusCodeType.ParameterError, "请求参数错误");
                }
                NlogLogger.Info("Sc支付请求：" + payRequest.ToJsonString());

                PayApiTypeEnum requestPayType = payRequest.PayType.ParseEnum<PayApiTypeEnum>();
                if (requestPayType == PayApiTypeEnum.NoPay)
                {
                    return toResponseError(StatusCodeType.ParameterError, "不支持该支付方式");
                }

                #endregion 检查提交信息

                #region 检查商户信息

                var merchant = _redisService.GetMerchantKeyValue(payRequest.MerchNo);
                if (merchant == null)
                {
                    return toResponseError(StatusCodeType.ParameterError, "商户号错误");
                }

                if (!RequestSignHelper.CheckScSign(payRequest, merchant.MerchantSecret))
                {
                    return toResponseError(StatusCodeType.ParameterError, "签名错误");
                }

                var payGroupId = merchant.PayGroupId;
                if (payGroupId <= 0)
                {
                    return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");
                }

                #endregion 检查商户信息

                //userid和userno如果没有默认空值
                payRequest.MerchNo = payRequest.MerchNo.Trim();
                payRequest.OrderNo = payRequest.OrderNo.Trim();
                payRequest.PayType = payRequest.PayType.Trim();
                payRequest.UserId = payRequest.UserId.IsNullOrEmpty() ? "" : payRequest.UserId.Trim();
                payRequest.UserNo = payRequest.UserNo.IsNullOrEmpty() ? "" : payRequest.UserNo.Trim();
                payRequest.NotifyUrl = payRequest.NotifyUrl.Trim();
                payRequest.Sign = payRequest.Sign.Trim();
                payRequest.Code = payRequest.Code.Replace(" ", "").Trim();
                payRequest.Seri = payRequest.Seri.Replace(" ", "").Trim();
                if (payRequest.UserNo.IsNullOrEmpty())
                {
                    payRequest.UserNo = payRequest.UserId;
                }

                #region 检查订单信息

                var checkPayOrder = await _payOrdersMongoService.GetPayOrderByOrderNumber(merchant.MerchantCode, payRequest.OrderNo);
                if (checkPayOrder != null)
                {
                    return toResponseError(StatusCodeType.ParameterError, "请不要重复提交订单");
                }

                #endregion 检查订单信息

                #region 支付方式获取

                var paygroup = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString());
                var scPayMent = paygroup.PayMents.Where(r => r.Type == PayMentTypeEnum.ScratchCards).ToList();
                if (scPayMent == null || !scPayMent.Any())
                {
                    return toResponseError(StatusCodeType.ParameterError, "商户支付未配置");
                }
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                var payment = scPayMent[rnd.Next(scPayMent.Count())];

                #endregion 支付方式获取

                #region 订单生成

                var bankOrderMark = "";
                if (merchant.MerchantSetting != null)
                {
                    if (!merchant.MerchantSetting.OrderBankRemark.IsNullOrEmpty())
                    {
                        bankOrderMark = merchant.MerchantSetting.OrderBankRemark;
                    }
                }
                string orderMark = OrderHelper.GenerateMark(bankOrderMark);

                var orderPayType = PayMentTypeEnum.ScratchCards;
                var rate = OrderHelper.GetRate(merchant.MerchantRate, orderPayType);
                var feeMoney = OrderHelper.GetFeeMoney(payRequest.Money, rate);
                PayOrdersMongoEntity payOrdersMongoEntity = new PayOrdersMongoEntity()
                {
                    MerchantCode = merchant.MerchantCode,
                    MerchantId = merchant.Id,
                    OrderNumber = payRequest.OrderNo,
                    OrderNo = OrderHelper.GenerateId(),
                    TransactionNo = payRequest.OrderNo,
                    OrderType = PayOrderOrderTypeEnum.Receive,
                    OrderStatus = PayOrderOrderStatusEnum.NotPaid,
                    OrderMoney = payRequest.Money,
                    Rate = rate,
                    FeeMoney = feeMoney,
                    OrderMark = orderMark,
                    PlatformCode = merchant.PlatformCode,
                    PayMentId = payment.Id,
                    UserId = payRequest.UserId,
                    UserNo = payRequest.UserNo,
                    NotifyUrl = payRequest.NotifyUrl,
                    PayType = orderPayType,
                    ScoreStatus = PayOrderScoreStatusEnum.NoScore,
                    ScoreNumber = 0,
                    PaymentChannel = payRequest.PayType.ParseEnum<PaymentChannelEnum>()
                };

                var orderId = await _payOrdersMongoService.AddAsync(payOrdersMongoEntity);

                #endregion 订单生成

                #region SC请求

                string siteUrl = AppSettings.Configuration["WebSite:BaseUrl"];
                var transactionid = OrderHelper.GetScTransactionid(merchant.Id, payRequest.OrderNo);
                ScratchCardRequest scratchCardRequest = new ScratchCardRequest()
                {
                    Amount = payRequest.Money,
                    TelcoName = payRequest.TelcoName,
                    Seri = payRequest.Seri,
                    Code = payRequest.Code,
                    Transactionid = transactionid,
                    CallUrl = siteUrl + "/api/CallBack/Doithecaoonline",
                };

                var response = await _scDoiTheCaoHelper.AddCard(payment, scratchCardRequest);
                if (response.success && response.code == 999)
                {
                    await _payOrdersMongoService.UpdateScInfoByOrderId(orderId, payRequest.Seri, payRequest.Code);
                    //更新订单信息
                    return toResponse(StatusCodeType.Success, "");
                }
                else
                {
                    //异常刮刮卡
                    return toResponse(StatusCodeType.Faild, response.message);
                }

                #endregion SC请求
            }
            catch (Exception ex)
            {
                NlogLogger.Error("SC请求订单异常:" + ex.ToString());
                return toResponse(StatusCodeType.Error, "请求异常");
            }
        }

        public async Task<IActionResult> BankPayNew(string orderid)
        {
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderid);
            if (payorder == null)
            {
                return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng không thành công" });
            }
            if (payorder.PayType != PayMentTypeEnum.ScratchCards)
            {
                //检查订单是否超过时间
                var dateNow = DateTime.Now;
                var orderTime = payorder.CreationTime;
                TimeSpan timeSpan = dateNow - orderTime;
                if (timeSpan.TotalMinutes > 40)
                {
                    return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng đã hết thời gian" });
                }
            }
            if (payorder.OrderStatus >= PayOrderOrderStatusEnum.Failed)
            {
                return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng đã hoàn thành" });
            }

            #endregion 检查商户支付信息

            var paytype = payorder.PayType;

            string qrcode = string.Empty;
            PayMentRedisModel? payment = payorder.PayMentId > 0 ? _redisService.GetPayMentInfoById(payorder.PayMentId) : null;

            if (payment == null)
            {
                return RedirectToAction("Error");
            }

            //VietQRPay vietQRPay = new VietQRPay();
            //vietQRPay.Init();
            //vietQRPay.amount = payorder.OrderMoney.ToString("F0");
            //var bankinfo = VietQRBankDic.Findbank(paytype);
            //vietQRPay.consumer.bankBin = bankinfo.bin;
            //vietQRPay.consumer.bankNumber = payment.CardNumber;
            //vietQRPay.additionalData.purpose = payorder.OrderMark;
            //string mbinfo = vietQRPay.build();
            var qrPay = QRPay.InitVietQR(
              bankBin: BankApp.BanksObject[QRPay.Findbank(paytype)].bin,
              bankNumber: payment.CardNumber,
              payorder.OrderMoney.ToString("F0"),
              payorder.OrderMark
            );
            var mbinfo = qrPay.Build();

            if (payorder.PaymentChannel == PaymentChannelEnum.MoMoPay)
            {
                qrcode = QrCodeHelper.GetQrCodeAsBase64(mbinfo, "#A50064");
            }
            else
            {
                qrcode = QrCodeHelper.GetQrCodeAsBase64(mbinfo);
            }
            //配置直接使用url,https://img.vietqr.io/image/970416-0368527514-qr_only.png?amount=200000&addInfo=MUA HANG J4H2846
            var qrtype = 0;
            var qrgenerate = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.PayOrderQRgenerate);
            if (!qrgenerate.IsNullOrEmpty() && qrgenerate == "1")
            {
                qrtype = 1;
                qrcode = "https://img.vietqr.io/image/" + BankApp.BanksObject[QRPay.Findbank(paytype)].bin + "-" + payment.CardNumber + "-qr_only.png?amount=" + payorder.OrderMoney.ToString("F0") + "&addInfo=" + payorder.OrderMark;
            }

            CultureInfo cultureInfo = new CultureInfo("vi-VN");
            PayViewModel payViewModel = new PayViewModel()
            {
                OrderId = payorder.ID,
                OrderNo = payorder.OrderNumber,
                Money = payorder.OrderMoney.ToString("C0", cultureInfo),
                OrderMoney = Convert.ToInt32(payorder.OrderMoney),
                QrCode = qrcode,
                QrType = qrtype,
                OrderMark = payorder.OrderMark,
                Phone = payment.CardNumber,
                Name = payment.FullName,
                PayType = payorder.PaymentChannel == PaymentChannelEnum.MoMoPay ? PayMentTypeEnum.MoMoPay : payment?.Type,
            };

            return View(payViewModel);
        }

        public async Task<IActionResult> Bank(string orderid)
        {
            try
            {
                #region 订单检查

                var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderid);
                if (payorder == null)
                {
                    return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng không thành công" });
                }

                if (payorder.PayType != PayMentTypeEnum.ScratchCards)
                {
                    //检查订单是否超过时间
                    var dateNow = DateTime.Now;
                    var orderTime = payorder.CreationTime;
                    TimeSpan timeSpan = dateNow - orderTime;
                    if (timeSpan.TotalMinutes > 40)
                    {
                        return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng đã hết thời gian" });
                    }
                }

                //判断订单是否支付，已支付返回提示
                if (payorder.OrderStatus >= PayOrderOrderStatusEnum.Failed)
                {
                    return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng đã hoàn thành" });
                }

                #endregion 订单检查

                //根据国家显示，可使用的银行卡类型
                var merchant = _redisService.GetMerchantKeyValue(payorder.MerchantCode);
                if (merchant == null)
                {
                    return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng không thành công" });
                }

                var paygroup = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString());

                #region 支付方式过滤，金额和余额

                var payments = paygroup.PayMents.Where(r => r.UseStatus == true && r.ShowStatus == PayMentStatusEnum.Show).ToList();
                var payList = await _payMentManageService.CheckBankPayMents(payments, payorder.OrderMoney);

                #endregion 支付方式过滤，金额和余额

                payList = payList.OrderBy(r => Guid.NewGuid()).ToList();
                var logoUrl = "";
                var title = "NsPay";
                if (merchant.MerchantSetting != null)
                {
                    if (!merchant.MerchantSetting.LogoUrl.IsNullOrEmpty())
                    {
                        logoUrl = merchant.MerchantSetting.LogoUrl;
                    }
                    if (!merchant.MerchantSetting.NsPayTitle.IsNullOrEmpty())
                    {
                        title = merchant.MerchantSetting.NsPayTitle;
                    }
                }
                CultureInfo cultureInfo = new CultureInfo("vi-VN");
                PayViewModel payViewModel = new PayViewModel()
                {
                    OrderId = payorder.ID,
                    OrderNo = payorder.OrderNumber,
                    Money = payorder.OrderMoney.ToString("C0", cultureInfo),
                    OrderMark = payorder.OrderMark,
                    PayMents = payList,
                    MerchantTitle = title,
                    MerchantLogoUrl = await GetMerchantLogoAsync(logoUrl)
                };

                return View(payViewModel);
            }
            catch (Exception ex)
            {
                NlogLogger.Error("页面加载错误：" + ex.ToString());
                return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng không thành công" });
            }
        }

        public async Task<IActionResult> BankPay(string orderid, string bankcode, int payid)
        {
            #region 订单检查

            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderid);
            if (payorder == null)
            {
                return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng không thành công" });
            }
            if (payorder.PayType != PayMentTypeEnum.ScratchCards)
            {
                //检查订单是否超过时间
                var dateNow = DateTime.Now;
                var orderTime = payorder.CreationTime;
                TimeSpan timeSpan = dateNow - orderTime;
                if (timeSpan.TotalMinutes > 40)
                {
                    return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng đã hết thời gian" });
                }
            }
            if (payorder.OrderStatus >= PayOrderOrderStatusEnum.Failed)
            {
                return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng đã hoàn thành" });
            }

            #endregion 订单检查

            PayMentTypeEnum paytype;
            string qrcode = string.Empty;
            PayMentRedisModel? payment = null;
            var payments = _redisService.GetPayMents();
            if (!bankcode.IsNullOrEmpty())
            {
                paytype = bankcode.ParseEnum<PayMentTypeEnum>();
                payment = payments.FirstOrDefault(r => r.Id == payid && r.Type == paytype);
                if (payment == null)
                {
                    return toResponseError(StatusCodeType.ParameterError, "Nạp tiền thất bại");
                }

                //订单同步支付方式
                await _payOrdersMongoService.UpdatePayOrderMentByOrderId(payorder.ID, payment.Id, paytype);
            }
            else
            {
                payment = payments.FirstOrDefault(r => r.Id == payorder.PayMentId);
                paytype = payment.Type;
            }
            if (payment == null)
            {
                return RedirectToAction("Error");
            }

            //VietQRPay vietQRPay = new VietQRPay();
            //vietQRPay.Init();
            //vietQRPay.amount = payorder.OrderMoney.ToString("F0");
            //var bankinfo = VietQRBankDic.Findbank(paytype);
            //vietQRPay.consumer.bankBin = bankinfo.bin;
            //vietQRPay.consumer.bankNumber = payment.CardNumber;
            //vietQRPay.additionalData.purpose = payorder.OrderMark;
            //string mbinfo = vietQRPay.build();
            var qrPay = QRPay.InitVietQR(
              bankBin: BankApp.BanksObject[QRPay.Findbank(paytype)].bin,
              bankNumber: payment.CardNumber,
              payorder.OrderMoney.ToString("F0"),
              payorder.OrderMark
            );
            var mbinfo = qrPay.Build();

            if (payorder.PaymentChannel == PaymentChannelEnum.MoMoPay)
            {
                qrcode = QrCodeHelper.GetQrCodeAsBase64(mbinfo, "#A50064");
            }
            else
            {
                qrcode = QrCodeHelper.GetQrCodeAsBase64(mbinfo);
            }
            //配置直接使用url,https://img.vietqr.io/image/970416-0368527514-qr_only.png?amount=200000&addInfo=MUA HANG J4H2846
            var qrtype = 0;
            var qrgenerate = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.PayOrderQRgenerate);
            if (!qrgenerate.IsNullOrEmpty() && qrgenerate == "1")
            {
                qrtype = 1;
                qrcode = "https://img.vietqr.io/image/" + BankApp.BanksObject[QRPay.Findbank(paytype)].bin + "-" + payment.CardNumber + "-qr_only.png?amount=" + payorder.OrderMoney.ToString("F0") + "&addInfo=" + payorder.OrderMark;
            }

            CultureInfo cultureInfo = new CultureInfo("vi-VN");
            PayViewModel payViewModel = new PayViewModel()
            {
                OrderId = payorder.ID,
                OrderNo = payorder.OrderNumber,
                Money = payorder.OrderMoney.ToString("C0", cultureInfo),
                OrderMoney = Convert.ToInt32(payorder.OrderMoney),
                QrCode = qrcode,
                QrType = qrtype,
                OrderMark = payorder.OrderMark,
                Phone = payment.CardNumber,
                Name = payment.FullName,
                PayType = payorder.PaymentChannel == PaymentChannelEnum.MoMoPay ? PayMentTypeEnum.MoMoPay : payment?.Type,
            };

            return View(payViewModel);
        }

        public async Task<IActionResult> ScratchCard(string orderid)
        {
            #region 订单检查

            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderid);
            if (payorder == null)
            {
                return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng không thành công" });
            }
            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Failed)
            {
                return RedirectToAction("Fail", "Pay");
            }
            if (payorder.OrderStatus == PayOrderOrderStatusEnum.TimeOut)
            {
                return RedirectToAction("Error", "Pay", new { msg = "Đơn hàng đã hết thời gian" });
            }
            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Completed)
            {
                return RedirectToAction("Success", "Pay");
            }

            #endregion 订单检查

            var typeCard = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.ScratchCardType).FromJsonString<List<NameValueRedisModel>>();

            return View(new ScratchCardViewModel { orderid = orderid, paymentid = payorder.PayMentId, money = payorder.OrderMoney, status = (int)payorder.OrderStatus, TypeCard = typeCard });
        }

        [HttpPost]
        public async Task<JsonResult> AddCard(ScratchCardRequest scratchCardRequest)
        {
            if (string.IsNullOrEmpty(scratchCardRequest.TelcoName))
            {
                return toResponse(StatusCodeType.Error, "Loại thẻ");
            }
            if (scratchCardRequest.Seri.IsNullOrEmpty())
            {
                return toResponse(StatusCodeType.Error, "Nhập số seri");
            }
            if (scratchCardRequest.Code.IsNullOrEmpty())
            {
                return toResponse(StatusCodeType.Error, "Nhập mã thẻ");
            }
            if (scratchCardRequest.Amount == 0)
            {
                return toResponse(StatusCodeType.Error, "Mệnh giá");
            }
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(scratchCardRequest.OrderId);
            if (payorder == null)
            {
                return toResponse(StatusCodeType.Error, "Không xác nhận lỗi sai");
            }
            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Completed)
            {
                return toResponse(StatusCodeType.Success, "Đơn hàng đã hoàn thành");//订单已成功
            }
            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Failed)
            {
                return toResponse(StatusCodeType.Error, "Đơn hàng không thành công");//订单已失败
            }
            var payment = _redisService.GetPayMentInfoById(payorder.PayMentId);
            if (payment == null)
            {
                return toResponse(StatusCodeType.Error, "Không xác nhận lỗi sai");//未知错误
            }

            //校验订单金额
            if (scratchCardRequest.Amount != payorder.OrderMoney)
            {
                return toResponse(StatusCodeType.Error, "Đơn hàng với số tiền không hợp lệ");
            }

            scratchCardRequest.Transactionid = OrderHelper.GetScTransactionid(payorder.MerchantId, payorder.OrderNumber);
            string siteUrl = AppSettings.Configuration["WebSite:BaseUrl"];
            scratchCardRequest.CallUrl = siteUrl + "/api/CallBack/Doithecaoonline";
            scratchCardRequest.Code = scratchCardRequest.Code.Replace(" ", "").Trim();
            scratchCardRequest.Seri = scratchCardRequest.Seri.Replace(" ", "").Trim();
            var response = await _scDoiTheCaoHelper.AddCard(payment, scratchCardRequest);
            if (response.success && response.code == 999)
            {
                await _payOrdersMongoService.UpdateScInfoByOrderId(payorder.ID, scratchCardRequest.Code, scratchCardRequest.Seri);
                return toResponse(StatusCodeType.Success, "");
            }
            else
            {
                await _payOrdersMongoService.UpdateOrderStatusByOrderId(payorder.ID, PayOrderOrderStatusEnum.Failed, 0, response.message);
                return toResponse(StatusCodeType.Error, response.message);
            }
        }

        [HttpPost]
        public async Task<JsonResult> CheckOrderStatus(string orderId)
        {
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderId);
            if (payorder == null)
            {
                return toResponse(StatusCodeType.Faild, "không có thứ tự!");
            }
            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Failed)
                return toResponseError(StatusCodeType.Faild, "thanh toán không thành công");

            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Completed)
            {
                return toResponse(StatusCodeType.Success, "");
            }
            else
            {
                return toResponse(StatusCodeType.Error, "Đang chờ kết quả");
            }
        }

        [HttpPost]
        public async Task<JsonResult> CheckCardStatus(ScratchCardRequest scratchCardRequest)
        {
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(scratchCardRequest.OrderId);
            if (payorder == null)
            {
                return toResponse(StatusCodeType.Error, "Không xác nhận lỗi sai");
            }
            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Completed)
            {
                return toResponse(StatusCodeType.Success, "");
            }
            else if (payorder.OrderStatus == PayOrderOrderStatusEnum.Failed)
            {
                return toResponse(StatusCodeType.Faild, payorder.ErrorMsg.IsNullOrEmpty() ? "" : payorder.ErrorMsg);
            }
            else
            {
                var payment = _redisService.GetPayMentInfoById(payorder.PayMentId);
                if (payment != null)
                {
                    //查询刮刮卡
                    var transactionid = OrderHelper.GetScTransactionid(payorder.MerchantId, payorder.OrderNumber);
                    ScratchCardRequest cardRequest = new ScratchCardRequest()
                    {
                        Amount = payorder.OrderMoney,
                        Code = payorder.ScCode,
                        Seri = payorder.ScSeri,
                        Transactionid = transactionid
                    };
                    var response = await _scDoiTheCaoHelper.CheckCard(payment, cardRequest);
                    if (response.success && response.code == 200 && response.data.FirstOrDefault()?.card_status == 2)
                    {
                        var detairesponse = response.data.FirstOrDefault();
                        if (detairesponse != null)
                        {
                            if (payorder.OrderMoney != detairesponse.card_real_amount)
                            {
                                await _payOrdersMongoService.UpdateOrderStatusByOrderId(payorder.ID, PayOrderOrderStatusEnum.Failed, detairesponse.card_real_amount, detairesponse.card_content);
                                return toResponseError(StatusCodeType.ParameterError, "Số tiền đơn hàng không khớp");
                            }
                            else
                            {
                                await _payOrdersMongoService.UpdateSuccesByOrderId(payorder.ID, payorder.OrderMoney);

                                //加入mq添加流水
                                var checkOrder = _redisService.GetMerchantBillOrder(payorder.MerchantCode, payorder.OrderNumber);
                                if (checkOrder.IsNullOrEmpty())
                                {
                                    _redisService.SetMerchantBillOrder(payorder.MerchantCode, payorder.OrderNumber);
                                    //var redisMqDto = new PayMerchantRedisMqDto()
                                    //{
                                    //    PayMqSubType = MQSubscribeStaticConsts.MerchantBillAddBalance,
                                    //    MerchantCode = payorder.MerchantCode,
                                    //    PayOrderId = payorder.ID,
                                    //};
                                    //_redisService.SetMerchantMqPublish(redisMqDto);
                                    await _kafkaProducer.ProduceAsync<PayOrderPublishDto>(KafkaTopics.PayOrder, payorder.ID, new PayOrderPublishDto()
                                    {
                                        MerchantCode = payorder.MerchantCode,
                                        PayOrderId = payorder.ID,
                                        TriggerDate = DateTime.Now,
                                        ProcessId = Guid.NewGuid().ToString()
                                    });
                                }

                                //回调
                                await _callBackService.CallBackPost(payorder.ID);

                                return toResponse(StatusCodeType.Success, "");
                            }
                        }
                    }
                }
            }
            return toResponse(StatusCodeType.Error, "等待结果");
        }

        public async Task<string> GetMerchantLogoAsync(string logoUrl)
        {
            if (string.IsNullOrEmpty(logoUrl))
                return "";
            var file = await _binaryObjectManager.GetFirstAsync(r => r.Id == Guid.Parse(logoUrl));
            var profilePictureContent = file == null ? "" : Convert.ToBase64String(file.Bytes);
            return profilePictureContent;
        }

        public IActionResult Error(string msg)
        {
            string message = msg;
            ErrorViewDto errorViewDto = new ErrorViewDto
            {
                ErrorMsg = message
            };
            return View(errorViewDto);
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Fail()
        {
            return View();
        }

        public IActionResult Result()
        {
            return View();
        }
    }
}
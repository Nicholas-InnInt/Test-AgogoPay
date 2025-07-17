using Abp.Extensions;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.BankInfo;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.ScratchCard;
using Neptune.NsPay.HttpExtensions.ScratchCard.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.VietQR;
using Neptune.NsPay.Web.Api.Helpers;
using Neptune.NsPay.Web.Api.Models;
using Neptune.NsPay.Web.Api.Models.Payment;
using System.Globalization;

namespace Neptune.NsPay.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IRedisService _redisService;
        private readonly IScDoiTheCaoHelper _scDoiTheCaoHelper;

        public PaymentController(IPayOrdersMongoService payOrdersMongoService, IRedisService redisService, IScDoiTheCaoHelper scDoiTheCaoHelper)
        {
            _payOrdersMongoService = payOrdersMongoService;
            _redisService = redisService;
            _scDoiTheCaoHelper = scDoiTheCaoHelper;
        }

        #region Bank

        [HttpGet("Bank")]
        public async Task<JsonResult> Bank(string orderId)
        {
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderId);
            var dateNow = DateTime.Now;

            if (payorder == null)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng không thành công", ProcessErrorCodeType.OrderFailed);
            }
            else if (payorder.OrderStatus >= PayOrderOrderStatusEnum.Failed)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng đã hoàn thành", ProcessErrorCodeType.OrderCompleted);
            }
            else if (payorder.PayType != PayMentTypeEnum.ScratchCards && (dateNow - payorder.CreationTime).TotalMinutes > 15)//  银行转账 15 分钟
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng đã hết thời gian", ProcessErrorCodeType.OrderExpired);
            }

            var secondToExpired = Convert.ToInt32(((15 * 60) - (dateNow - payorder.CreationTime).TotalSeconds));
            PayMentRedisModel? payment = payorder.PayMentId > 0 ? _redisService.GetPayMentInfoById(payorder.PayMentId) : null;

            PayMentTypeEnum paytype = payorder.PayType;

            var qrtype = 0;
            string qrcode = string.Empty;
            var qrgenerate = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.PayOrderQRgenerate);
            if (!qrgenerate.IsNullOrEmpty() && qrgenerate == "1")
            {
                qrtype = 1;
                qrcode = "https://img.vietqr.io/image/" + BankApp.BanksObject[QRPay.Findbank(paytype)].bin + "-" + payment.CardNumber + "-qr_only.png?amount=" + payorder.OrderMoney.ToString("F0") + "&addInfo=" + payorder.OrderMark;
            }
            else
            {
                var qrPay = QRPay.InitVietQR(
                    bankBin: BankApp.BanksObject[QRPay.Findbank(paytype)].bin,
                    bankNumber: payment.CardNumber,
                    payorder.OrderMoney.ToString("F0"),
                    payorder.OrderMark
                );
                var mbinfo = qrPay.Build();
                qrcode = QrCodeHelper.GetQrCodeAsBase64(mbinfo);
            }

            CultureInfo cultureInfo = new CultureInfo("vi-VN");

            return toResponse<PayBankResponse>(new PayBankResponse()
            {
                SecondsToExpired = secondToExpired,
                OrderId = payorder.ID,
                OrderNo = payorder.OrderNumber,
                Money = payorder.OrderMoney.ToString("C0", cultureInfo),
                OrderMoney = Convert.ToInt32(payorder.OrderMoney),
                QrCode = qrcode,
                QrType = qrtype,
                OrderMark = payorder.OrderMark,
                CardNumber = payment.CardNumber,
                FullName = payment.FullName,
                BankName = (payment?.Type).ToString(),
                PayType = (payorder.PaymentChannel == PaymentChannelEnum.MoMoPay ? PayMentTypeEnum.MoMoPay : payment?.Type).ToString(),
            });
        }

        [HttpGet("Momo")]
        public async Task<JsonResult> Momo(string orderId)
        {
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderId);
            var dateNow = DateTime.Now;

            if (payorder == null)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng không thành công", ProcessErrorCodeType.OrderFailed);
            }
            else if (payorder.OrderStatus >= PayOrderOrderStatusEnum.Failed)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng đã hoàn thành", ProcessErrorCodeType.OrderCompleted);
            }
            else if (payorder.PayType != PayMentTypeEnum.ScratchCards && (dateNow - payorder.CreationTime).TotalMinutes > 15)//  银行转账 15 分钟
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng đã hết thời gian", ProcessErrorCodeType.OrderExpired);
            }

            var secondToExpired = Convert.ToInt32(((15 * 60) - (dateNow - payorder.CreationTime).TotalSeconds));
            PayMentRedisModel? payment = payorder.PayMentId > 0 ? _redisService.GetPayMentInfoById(payorder.PayMentId) : null;
            PayMentTypeEnum paytype = payorder.PayType;

            var qrtype = 0;
            string qrcode = string.Empty;
            var qrgenerate = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.PayOrderQRgenerate);
            if (!qrgenerate.IsNullOrEmpty() && qrgenerate == "1")
            {
                qrtype = 1;
                qrcode = "https://img.vietqr.io/image/" + BankApp.BanksObject[QRPay.Findbank(paytype)].bin + "-" + payment.CardNumber + "-qr_only.png?amount=" + payorder.OrderMoney.ToString("F0") + "&addInfo=" + payorder.OrderMark;
            }
            else
            {
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
            }

            CultureInfo cultureInfo = new CultureInfo("vi-VN");

            return toResponse<PayBankResponse>(new PayBankResponse()
            {
                SecondsToExpired = secondToExpired,
                OrderId = payorder.ID,
                OrderNo = payorder.OrderNumber,
                Money = payorder.OrderMoney.ToString("C0", cultureInfo),
                OrderMoney = Convert.ToInt32(payorder.OrderMoney),
                QrCode = qrcode,
                QrType = qrtype,
                OrderMark = payorder.OrderMark,
                CardNumber = payment.CardNumber,
                FullName = payment.FullName,
                BankName = (payorder.PaymentChannel == PaymentChannelEnum.MoMoPay ? PayMentTypeEnum.MoMoPay : payment?.Type).ToString(),
                PayType = (payorder.PaymentChannel == PaymentChannelEnum.MoMoPay ? PayMentTypeEnum.MoMoPay : payment?.Type).ToString(),
            });
        }

        [HttpGet("Sc")]
        public async Task<JsonResult> Sc(string orderId)
        {
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderId);
            var dateNow = DateTime.Now;
            if (payorder == null)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng không thành công", ProcessErrorCodeType.OrderFailed);
            }
            else if (payorder.OrderStatus == PayOrderOrderStatusEnum.Failed)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "", ProcessErrorCodeType.OrderFailed);
            }
            else if (payorder.OrderStatus == PayOrderOrderStatusEnum.TimeOut)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng đã hết thời gian", ProcessErrorCodeType.OrderExpired);
            }
            else if (payorder.OrderStatus == PayOrderOrderStatusEnum.Completed)
            {
                return toResponseWithErrorCode(StatusCodeType.Success, "", ProcessErrorCodeType.OrderCompleted);
            }
            else if ((dateNow - payorder.CreationTime).TotalMinutes > 15)//  银行转账 15 分钟
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng đã hết thời gian", ProcessErrorCodeType.OrderExpired);
            }
            var secondToExpired = Convert.ToInt32(((15 * 60) - (dateNow - payorder.CreationTime).TotalSeconds));
            var typeCard = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.ScratchCardType).FromJsonString<List<NameValueRedisModel>>();
            CultureInfo cultureInfo = new CultureInfo("vi-VN");

            return toResponse(new ScratchCardResponse
            {
                SecondsToExpired = secondToExpired,
                OrderId = orderId,
                OrderNo = payorder.OrderNo,
                OrderMoney = Convert.ToInt32(payorder.OrderMoney),
                Money = payorder.OrderMoney.ToString("C0", cultureInfo),
                TypedCards = typeCard,
                PaymentId = payorder.PayMentId
            });
        }

        [HttpPost("CheckOrderStatus")]
        public async Task<JsonResult> CheckOrderStatus(string orderId)
        {
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderId);
            if (payorder == null)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "không có thứ tự!", ProcessErrorCodeType.OrderFailed);
            }
            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Failed)
                return toResponseWithErrorCode(StatusCodeType.Error, "thanh toán không thành công", ProcessErrorCodeType.OrderFailed);

            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Completed)
            {
                return toResponseWithErrorCode(StatusCodeType.Success, "");
            }
            else
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đang chờ kết quả", ProcessErrorCodeType.WaitingOrderResult);
            }
        }

        [HttpPost("AddCard")]
        public async Task<JsonResult> AddCard(ScratchCardRequest scratchCardRequest)
        {
            if (string.IsNullOrEmpty(scratchCardRequest.TelcoName))
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Loại thẻ", ProcessErrorCodeType.ParamError);
            }
            if (scratchCardRequest.Seri.IsNullOrEmpty())
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Nhập số seri", ProcessErrorCodeType.ParamError);
            }
            if (scratchCardRequest.Code.IsNullOrEmpty())
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Nhập mã thẻ", ProcessErrorCodeType.ParamError);
            }
            if (scratchCardRequest.Amount == 0)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Mệnh giá", ProcessErrorCodeType.ParamError);
            }
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(scratchCardRequest.OrderId);
            if (payorder == null)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Không xác nhận lỗi sai", ProcessErrorCodeType.UnknownError);
            }
            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Completed)
            {
                return toResponseWithErrorCode(StatusCodeType.Success, "Đơn hàng đã hoàn thành", ProcessErrorCodeType.OrderCompleted);//订单已成功
            }
            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Failed)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng không thành công", ProcessErrorCodeType.OrderFailed);//订单已失败
            }
            var payment = _redisService.GetPayMentInfoById(payorder.PayMentId);
            if (payment == null)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Không xác nhận lỗi sai", ProcessErrorCodeType.UnknownError);//未知错误
            }

            //校验订单金额
            if (scratchCardRequest.Amount != payorder.OrderMoney)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng với số tiền không hợp lệ", ProcessErrorCodeType.OrderAmountNotMatch);
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
                return toResponseWithErrorCode(StatusCodeType.Success, "");
            }
            else
            {
                await _payOrdersMongoService.UpdateOrderStatusByOrderId(payorder.ID, PayOrderOrderStatusEnum.Failed, 0, response.message);
                return toResponseWithErrorCode(StatusCodeType.Error, response.message, ProcessErrorCodeType.UnknownError);
            }
        }

        #endregion Bank

        #region Cryptocurrency

        [HttpGet("Crypto")]
        public async Task<JsonResult> Crypto(string orderId)
        {
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderId);
            var expiredInSeconds = 15 * 60; // 15 minutes in seconds
            var dateNow = DateTime.Now;

            if (payorder == null)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng không thành công", ProcessErrorCodeType.OrderFailed);
            }
            else if (payorder.OrderStatus >= PayOrderOrderStatusEnum.Failed)
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng đã hoàn thành", ProcessErrorCodeType.OrderCompleted);
            }
            else if ((dateNow - payorder.CreationTime).TotalSeconds > expiredInSeconds) // 银行转账 15分钟
            {
                return toResponseWithErrorCode(StatusCodeType.Error, "Đơn hàng đã hết thời gian", ProcessErrorCodeType.OrderExpired);
            }

            var payment = payorder.PayMentId > 0 ? _redisService.GetPayMentInfoById(payorder.PayMentId) : null;

            var conversionRate = payorder.ConversionRate ?? 0;

            return toResponse(new CryptoResponse
            {
                OrderId = payorder.ID,
                OrderNo = payorder.OrderNumber,
                Money = $"{payorder.OrderMoney} USDT",
                OrderMoney = payorder.OrderMoney,
                ConversionRate = conversionRate,
                ConvertedMoney = (payorder.UnproccessMoney ?? 0).ToString("C0", new CultureInfo("vi-VN")),
                QrCode = QrCodeHelper.GetQrCodeAsBase64(payment.CryptoWalletAddress),
                PayType = payorder.PayType.ToString(),
                WalletAddress = payment.CryptoWalletAddress,
                SecondsToExpired = Convert.ToInt32(expiredInSeconds - (dateNow - payorder.CreationTime).TotalSeconds),
            });
        }

        [HttpPost("CheckCryptoOrderStatus")]
        public async Task<JsonResult> CheckCryptoOrderStatus(string orderId)
        {
            var payorder = await _payOrdersMongoService.GetPayOrderByOrderId(orderId);
            if (payorder is null)
                return toResponseWithErrorCode(StatusCodeType.Error, "không có thứ tự!", ProcessErrorCodeType.OrderFailed);

            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Failed)
                return toResponseWithErrorCode(StatusCodeType.Error, "thanh toán không thành công", ProcessErrorCodeType.OrderFailed);

            if (payorder.OrderStatus == PayOrderOrderStatusEnum.Completed)
                return toResponseWithErrorCode(StatusCodeType.Success, "");

            return toResponseWithErrorCode(StatusCodeType.Error, "Đang chờ kết quả", ProcessErrorCodeType.WaitingOrderResult);
        }

        #endregion Cryptocurrency
    }
}
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Neptune.NsPay.Commons;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.Web.PayMonitorApi.Helpers;
using Neptune.NsPay.Web.PayMonitorApi.Models;
using Neptune.NsPay.Web.PayMonitorApi.Service;

namespace Neptune.NsPay.Web.PayMonitorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly static string Sign = "dcaebf01858f33594ff3074fb7b81d73";
        private readonly IPushUpdateService _pushUpdateService;
        private readonly IRedisService _redisService;
        private readonly IPayMonitorCommonHelpers _payMonitorCommonHelpers;
        public NotificationController(
            IPushUpdateService pushUpdateService , IRedisService redisService , IPayMonitorCommonHelpers payMonitorCommonHelpers)
        {
            _pushUpdateService = pushUpdateService;
            _redisService = redisService;
            _payMonitorCommonHelpers = payMonitorCommonHelpers;
        }

        /// <summary>
        /// 设置余额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/UpdateBalanceAndNotify")]
        public async Task<JsonResult> SetBalance([FromBody] BalanceInput request)
        {
            NlogLogger.Trace("余额更新：" + request.ToJsonString());
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });
            if (request.PayMentId <= 0)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == request.PayMentId);
            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            var checkBalance = _redisService.GetBalanceByPaymentId(request.PayMentId);
            if (checkBalance != null)
            {
                checkBalance.Balance2 = request.Balance;
                checkBalance.UpdateTime = DateTime.Now;
                _redisService.SetBalance(payment.Id, checkBalance);
                await _pushUpdateService.BalanceChanged(new BalanceUpdateNotification() { PayMentId = checkBalance.PayMentId, Balance = checkBalance.Balance2, UpdatedTime = checkBalance.UpdateTime });

            }
            else
            {
                var balance = new BankBalanceModel()
                {
                    PayMentId = payment.Id,
                    Type = payment.Type,
                    UserName = payment.Phone,
                    Balance = 0,
                    Balance2 = request.Balance,
                    UpdateTime = DateTime.Now
                };
                _redisService.SetBalance(payment.Id, balance);
               await _pushUpdateService.BalanceChanged(new BalanceUpdateNotification() { PayMentId = balance.PayMentId, Balance = balance.Balance2, UpdatedTime = balance.UpdateTime });
            }


            if(await _payMonitorCommonHelpers.UpdatePaymentUseState(payment.Id , request.Balance))
            {
                await _payMonitorCommonHelpers.NotifyMerchantPaymentChanged(payment.Id);
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

    }
}

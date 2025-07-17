using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.PayOnline;
using Neptune.NsPay.HttpExtensions.PayOnline.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.RedisExtensions;

namespace Neptune.NsPay.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FyCallBackController : BaseController
    {
        private readonly IPayOnlineHelper _payOnlineHelper;
		private readonly IPayOrdersMongoService _payOrdersMongoService;
		private readonly IRedisService _redisService;
		public FyCallBackController(
			IRedisService redisService,
			IPayOrdersMongoService payOrdersMongoService,
			IPayOnlineHelper payOnlineHelper)
        {
			_payOrdersMongoService = payOrdersMongoService;
			_payOnlineHelper = payOnlineHelper;
			_redisService = redisService;
		}

		[Route("~/FyCallBack/FyPay")]
		[HttpPost]
		public async Task<string> FyPay([FromBody] FyCallBackModel request) 
        {
			NlogLogger.Warn("FyPay call back result：" + request.ToJsonString());
			try
            {
                if (request.status == 10000 ) 
                {
					var result = request.result;
					var payOrderInfo = await _payOrdersMongoService.GetPayOrderByOrderNumberAmt(result.amount, result.orderid);
					if (payOrderInfo != null)
					{
						var uid = "";
						var merchant = _redisService.GetMerchantKeyValue(payOrderInfo.MerchantCode);
                        if (merchant != null)
                        {
							var paygroup = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString());
							if (paygroup != null)
							{
								var fyPayMent = paygroup.PayMents.Where(r => r.Id == payOrderInfo.PayMentId).FirstOrDefault();

								if (fyPayMent != null)
								{
									uid = fyPayMent.CompanyKey;
									var getFyPayHistoryRequest = new GetFyPayHistoryRequest()
									{
										Uid = uid,
										OrderId = result.orderid,
										Sign = request.sign
									};
									var payHistoryResponse = await _payOnlineHelper.GetFyPayHistory(fyPayMent.Gateway, getFyPayHistoryRequest);
									if (payHistoryResponse != null)
									{
										if (payHistoryResponse.status == 10000)
											return "success";
									}
								}
								
							}
						}
					}
				}

				return "";
			}
            catch (Exception ex)
            {
				NlogLogger.Warn("FyPay 支付异常：" + ex);
				return "";
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

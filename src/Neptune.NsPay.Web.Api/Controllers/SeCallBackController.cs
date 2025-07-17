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
    public class SeCallBackController : BaseController
    {
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayOnlineHelper _payOnlineHelper;
		private readonly IRedisService _redisService;
		public SeCallBackController(IPayOrdersMongoService payOrdersMongoService,
			IPayOnlineHelper payOnlineHelper,
			IRedisService redisService)
        {
            _payOrdersMongoService = payOrdersMongoService;
			_payOnlineHelper = payOnlineHelper;
			_redisService = redisService;
		}

		[Route("~/SeCallBack/SePay")]
		[HttpPost]
		public async Task<string> SePay([FromBody] SeCallbackModel request) 
        {
			NlogLogger.Warn("SePay call back result：" + request.ToJsonString());
			try
            {
                if (request.code == "10000" && request.result == "Success") 
                {
					var payOrderInfo = await _payOrdersMongoService.GetPayOrderByOrderNumberAmt(decimal.Parse(request.amount), request.tradeNo);
					if (payOrderInfo != null)
					{
						var memberId = "";
						var merchant = _redisService.GetMerchantKeyValue(payOrderInfo.MerchantCode);
						if (merchant != null)
						{
							var paygroup = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString());
							if (paygroup != null)
							{
								var sePayMent = paygroup.PayMents.Where(r => r.Id == payOrderInfo.PayMentId).FirstOrDefault();
								if (sePayMent != null)
								{
									memberId = sePayMent.CompanyKey;
									var getPayHistoryRequest = new GetSePayHistoryRequest()
									{
										memberId = memberId,
										orderNumber = request.tradeNo,
										sign = request.sign
									};
									var payHistoryResponse = await _payOnlineHelper.GetSePayHistory(sePayMent.Gateway, getPayHistoryRequest);
									if (payHistoryResponse != null)
									{
										if (payHistoryResponse.code == "10000")
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
                return "";
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

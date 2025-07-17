using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Commons;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Web.Api.Models;
using RestSharp;

namespace Neptune.NsPay.Web.Api.Controllers
{
    public class TestController : BaseController
    {
        private readonly IRedisService _redisService;

        public TestController(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public IActionResult Index()
        {
            var isTest = Convert.ToInt32(AppSettings.Configuration["WebSite:IsTest"]);
            if (isTest == 0)
            {
                return toResponseError(StatusCodeType.ParameterError, "请求参数错误!");
            }
            return View();
        }

        public IActionResult InOrder(PlatformPayRequest payRequest)
        {
            var isTest = Convert.ToInt32(AppSettings.Configuration["WebSite:IsTest"]);
            if (isTest == 0)
            {
                return toResponseError(StatusCodeType.ParameterError, "请求参数错误!");
            }
            payRequest.NotifyUrl = "http://localhost:9999/";
            var merchant = _redisService.GetMerchantKeyValue(payRequest.MerchNo);
            if (merchant == null)
            {
                return toResponseError(StatusCodeType.ParameterError, "商户号错误");
            }
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "MerchNo".ToLower(), payRequest.MerchNo.ToLower() },
                { "OrderNo".ToLower(), payRequest.OrderNo.ToLower() },
                { "Money".ToLower(), payRequest.Money.ToString().ToLower() },
                { "PayType".ToLower(), payRequest.PayType.ToLower() },
                { "NotifyUrl".ToLower(), payRequest.NotifyUrl.ToLower() }
            };
            var param = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            string sign = SignHelper.GetSignContent(param) + "&secret=" + merchant.MerchantSecret.ToLower();
            payRequest.Sign = MD5Helper.MD5Encrypt32(sign).ToLower();

            var payApiType = payRequest.PayType.ParseEnum<PayApiTypeEnum>();

            var url = AppSettings.Configuration["WebSite:BaseUrl"];
            var payRoutePath = payApiType is PayApiTypeEnum.USDT_TRC20 or PayApiTypeEnum.USDT_ERC20 ? "Pay/CryptoPay" : "Pay/Index";
            var client = new RestClient(url + payRoutePath);
            var request = new RestRequest()
                    .AddParameter("application/json", payRequest.ToJsonString(), ParameterType.RequestBody);
            var response = client.ExecutePost(request);
            return Json(response.Content);
        }

        public IActionResult OutOrder(PlatformTransferRequest transferRequest)
        {
            var isTest = Convert.ToInt32(AppSettings.Configuration["WebSite:IsTest"]);
            if (isTest == 0)
            {
                return toResponseError(StatusCodeType.ParameterError, "请求参数错误!");
            }
            transferRequest.NotifyUrl = "http://localhost:9999/";
            var merchant = _redisService.GetMerchantKeyValue(transferRequest.MerchNo);
            if (merchant == null)
            {
                return toResponseError(StatusCodeType.ParameterError, "商户号错误");
            }
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "MerchNo".ToLower(), transferRequest.MerchNo.ToLower() },
                { "OrderNo".ToLower(), transferRequest.OrderNo.ToLower() },
                { "Money".ToLower(), transferRequest.Money.ToString().ToLower() },
                { "BankAccNo".ToLower(), transferRequest.BankAccNo.ToString().ToLower() },
                { "BankAccName".ToLower(), transferRequest.BankAccName.ToString().ToLower() },
                { "BankName".ToLower(), transferRequest.BankName.ToString().ToLower() },
                { "NotifyUrl".ToLower(), transferRequest.NotifyUrl.ToLower() }
            };
            var param = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            string sign = SignHelper.GetSignContent(param) + "&secret=" + merchant.MerchantSecret.ToLower();
            transferRequest.Sign = MD5Helper.MD5Encrypt32(sign).ToLower();

            var url = AppSettings.Configuration["WebSite:BaseUrl"];
            var client = new RestClient(url + "Transfer/Index");
            var request = new RestRequest()
                    .AddParameter("application/json", transferRequest.ToJsonString(), ParameterType.RequestBody);
            var response = client.ExecutePost(request);
            return Json(response.Content);
        }
    }
}
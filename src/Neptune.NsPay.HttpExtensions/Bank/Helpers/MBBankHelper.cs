using Abp.Extensions;
using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.RedisExtensions;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class MBBankHelper : IMBBankHelper
    {
        private readonly static string DoLoginUrl = "https://online.mbbank.com.vn/api/retail_web/internetbanking/doLogin";
        private readonly static string TransactionHistoryUrl = "https://online.mbbank.com.vn/api/retail-transactionms/transactionms/get-account-transaction-history";
        private readonly static string Authorization = "Basic RU1CUkVUQUlMV0VCOlNEMjM0ZGZnMzQlI0BGR0AzNHNmc2RmNDU4NDNm";
        private readonly static string DeviceIdCommon = "bi2tsnvu-mbib-0000-0000-2022070416582575";
        private readonly static string socksUrl = AppSettings.Configuration["SocksUrl"];
        private readonly static int socksPort = AppSettings.Configuration["SocksPort"].ToInt();
        private readonly static string socksusername = AppSettings.Configuration["SocksUsername"];
        private readonly static string sockspassword = AppSettings.Configuration["SocksPassword"];

        private readonly IRedisService _redisService;
        public MBBankHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public string GetLogin(string userid, string password, string captcha)
        {
            password = MD5Helper.MD5Encrypt32(password).ToLower();
            LoginRequest captchaImageRequest = new LoginRequest()
            {
                userId = userid,
                password = password,
                captcha = captcha,
                deviceIdCommon = DeviceIdCommon,
                refNo = CommonHelper.GetDataString()
            };
            var data = captchaImageRequest.ToJsonString();

            NlogLogger.Warn("账户:" + userid + "，MB银行登录参数：" + data);

            var client = new RestClient(DoLoginUrl);
            var request = new RestRequest()
                .AddHeader("Authorization", Authorization)
                .AddParameter("application/json", data, ParameterType.RequestBody);
            var response = client.ExecutePost(request);
            NlogLogger.Warn("账户:" + userid + "，MB银行登录返回：" + response.Content);
            if (response == null || response.Content.IsNullOrEmpty())
            {
                return string.Empty;
            }
            var captchaImageResponse = response.Content.FromJsonString<LoginResponse>();
            if (captchaImageResponse == null || captchaImageResponse.result.ok == false)
                return string.Empty;
            return captchaImageResponse.sessionId;
        }

        public async Task<List<TransactionHistoryItem>> GetTransactionHistory(string account, string cardNumber)
        {
            var sessionid = GetSessionId(cardNumber);
            if (sessionid == null || sessionid.SessionId.IsNullOrEmpty())
            {
                return null;
            }
            var cookies = sessionid.SessionId.Replace("\"", "");
            //MB一次返回全部太多了，正常时间只获取当天，要快天时获取两天了，维持一小时
            DateTime date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
            DateTime date2 = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
            var checkTime = date.Hour;
            if (checkTime == 0 || checkTime == 23)
            {
                date = DateTime.Now.AddDays(-1);
            }
            var sessionidStr = "";
            var deviceId = "";
            var arr = sessionid.SessionId.Split('&');
            if (arr.Length > 0)
            {
                if (arr.Length == 2)
                {
                    sessionidStr = arr[0];
                    deviceId = arr[1];
                }
                else
                {
                    sessionidStr = arr[0];
                    deviceId = DeviceIdCommon;
                }
            }
            else
            {
                sessionidStr = cookies;
                deviceId = DeviceIdCommon;
            }

            var historyRequest = new TransactionHistoryRequest()
            {
                accountNo = cardNumber,
                deviceIdCommon = deviceId,
                fromDate = date.ToString("dd/MM/yyyy"),
                refNo = account + "-" + CommonHelper.GetDataString(),
                sessionId = sessionidStr,
                toDate = date2.ToString("dd/MM/yyyy"),
            };

            var data = historyRequest.ToJsonString();

            NlogLogger.Info("MB BANK账户：" + account + "订单数据：" + data);
            var client = new RestClient(TransactionHistoryUrl);
            if (!socksUrl.IsNullOrEmpty())
            {
                WebProxy webProxy = new WebProxy(socksUrl, socksPort);
                webProxy.BypassProxyOnLocal = true;
                webProxy.Credentials = new NetworkCredential(socksusername, sockspassword);
                client = new RestClient(new RestClientOptions(TransactionHistoryUrl)
                {
                    Proxy = webProxy
                });
            }
            var request = new RestRequest()
                    .AddHeader("Authorization", Authorization)
                    .AddHeader("Deviceid", deviceId)
                    .AddHeader("Refno", historyRequest.refNo)
                    .AddParameter("application/json", data, ParameterType.RequestBody);
            request.Timeout = 2 * 60 * 1000;
            var response = await client.ExecutePostAsync(request);
            NlogLogger.Info("MB BANK账户：" + account + "请求信息：" + response.Content);
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (!response.Content.IsNullOrEmpty())
                    {
                        //NlogLogger.Info("MB BANK账户：" + account + "订单详情：" + response.Content);
                        var detailResponse = JsonConvert.DeserializeObject<TransactionHistoryResponse>(response.Content);
                        if (detailResponse != null)
                        {
                            if (detailResponse.result.ok == true)
                            {
                                return detailResponse.transactionHistoryList;
                            }
                        }
                    }
                }
                else
                {
                    NlogLogger.Error("账户：" + account + "MB BANK获取错误：" + response.ToJsonString());
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + account + "MB BANK获取异常：" + ex);
            }
            sessionid.ErrorCount = sessionid.ErrorCount + 1;
            SetSessionId(cardNumber, sessionid);
            //_redisService.RemoveRedisValue(CommonHelper.GetMBBankCacheBankKey(cardNumber));
            return null;
        }

        public MbBankLoginModel GetSessionId(string account)
        {
            return _redisService.GetRedisValue<MbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.MBBank, account));
        }

        public void SetSessionId(string account, MbBankLoginModel sessionid)
        {
            _redisService.AddRedisValue<MbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.MBBank, account), sessionid);
        }

        public void RemoveSessionId(string cardNumber)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.MBBank, cardNumber));
        }

        public string GetLastRefNoKey(string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.MBBank, account));
        }

        public void SetLastRefNoKey(string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.MBBank, account), refno);
        }
    }
}
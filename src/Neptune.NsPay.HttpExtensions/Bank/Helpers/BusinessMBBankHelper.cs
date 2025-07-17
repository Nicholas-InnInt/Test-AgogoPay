using Abp.Extensions;
using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.RedisExtensions;
using RestSharp;
using System.Net;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class BusinessMBBankHelper : IBusinessMBBankHelper
    {
        private readonly static string TransactionHistoryUrl = "https://ebank.mbbank.com.vn/corp/transaction/v2/getTransactionHistoryV3";
        private readonly IRedisService _redisService;

        private readonly static string socksUrl = AppSettings.Configuration["SocksUrl"];
        private readonly static int socksPort = AppSettings.Configuration["SocksPort"].ToInt();
        private readonly static string socksusername = AppSettings.Configuration["SocksUsername"];
        private readonly static string sockspassword = AppSettings.Configuration["SocksPassword"];
        public BusinessMBBankHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<List<BusinessMBBankTransactionHistoryList>> GetTransactionHistory(string account, string cardNumber, string name)
        {
            var cacheToken = GetSessionId(cardNumber);
            if (cacheToken != null)
            {
                WebProxy webProxy = new WebProxy(socksUrl, socksPort);
                webProxy.BypassProxyOnLocal = true;
                webProxy.Credentials = new NetworkCredential(socksusername, sockspassword);
                var client = new RestClient(TransactionHistoryUrl);
                if (!socksUrl.IsNullOrEmpty())
                {
                    client = new RestClient(new RestClientOptions(TransactionHistoryUrl)
                    {
                        Proxy = webProxy
                    });
                }
                DateTime startTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
                DateTime endTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
                //var checkTime = startTime.Hour;
                //if (checkTime == 0 || checkTime == 23)
                //{
                //    startTime = DateTime.Now.AddDays(-1);
                //}
                startTime = startTime.AddMinutes(-30);

                var flag = 0;
                List<BusinessMBBankTransactionHistoryList> historyLists = new List<BusinessMBBankTransactionHistoryList>();
                for (int i = 1; i <= 3; i++)
                {
                    var timeStr = endTime.ToString("yyyyMMddHHmmssff");
                    int? top = null;
                    var paramData = new
                    {
                        accountNo = cardNumber,
                        accountName = name,
                        fromDate = startTime.ToString("dd/MM/yyyy HH:MM"),
                        toDate = endTime.ToString("dd/MM/yyyy 23:59"),
                        page = i,
                        size = 15,
                        top = top,
                        currency = "VND",
                        refNo = account + "-" + timeStr
                    };
                    var data = paramData.ToJsonString();
                    var request = new RestRequest()
                        .AddHeader("Authorization", cacheToken.SessionId)
                        .AddHeader("Host", "ebank.mbbank.com.vn")
                        .AddHeader("Origin", "https://ebank.mbbank.com.vn")
                        .AddHeader("Referer", "https://ebank.mbbank.com.vn/cp/account-info/transaction-inquiry")
                        .AddHeader("User-Agent", cacheToken.UserAgent)
                        .AddHeader("X-Request-Id", account + "-" + timeStr)
                        .AddHeader("Biz-Platform", cacheToken.BizPlatform)
                        .AddHeader("Biz-Trace-Id", cacheToken.BizTraceId)
                        .AddHeader("Biz-Tracking", cacheToken.BizTracking)
                        .AddHeader("Biz-Version", cacheToken.BizVersion)
                        .AddHeader("Elastic-Apm-Traceparent", cacheToken.ElasticApmTraceparent)
                        .AddParameter("application/json", data, ParameterType.RequestBody);
                    var response = await client.ExecutePostAsync(request);
                    try
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            if (!response.Content.IsNullOrEmpty())
                            {
                                var detailResponse = response.Content.FromJsonString<BusinessMBBankTransactionHistoryResponse>();
                                if (detailResponse != null)
                                {
                                    if (detailResponse.result.ok == true)
                                    {
                                        flag = 1;
                                        historyLists.AddRange(detailResponse.transactionHistoryList);
                                    }
                                }
                            }
                        }
                        else
                        {
                            NlogLogger.Error("账户：" + account + "BusinessMb获取错误：" + response.ToJsonString());
                        }
                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error("账户：" + account + "BusinessMb获取异常：" + ex);
                    }
                }
                if (flag == 0)
                {
                    return null;
                }
                return historyLists;
            }
            return null;
        }

        public BusinessMBBankLoginModel GetSessionId(string account)
        {
            return _redisService.GetRedisValue<BusinessMBBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BusinessMbBank, account));
        }

        public void SetSessionId(string account, BusinessMBBankLoginModel sessionid)
        {
            _redisService.AddRedisValue<BusinessMBBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BusinessMbBank, account), sessionid);
        }

        public void RemoveToken(string account)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BusinessMbBank, account));
        }

        public string GetLastRefNoKey(string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.BusinessMbBank, account));
        }

        public void SetLastRefNoKey(string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.BusinessMbBank, account), refno);
        }
    }
}
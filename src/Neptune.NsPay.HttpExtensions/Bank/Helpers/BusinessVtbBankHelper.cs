using Abp.Extensions;
using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.RedisExtensions;
using RestSharp;
using System.Net;
using System.Text;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class BusinessVtbBankHelper : IBusinessVtbBankHelper
    {
        private readonly static string TransactionHistoryUrl = "https://efast.vietinbank.vn/api/v1/account/history";
        private readonly IRedisService _redisService;

        private readonly static string socksUrl = AppSettings.Configuration["SocksUrl"];
        private readonly static int socksPort = AppSettings.Configuration["SocksPort"].ToInt();
        private readonly static string socksusername = AppSettings.Configuration["SocksUsername"];
        private readonly static string sockspassword = AppSettings.Configuration["SocksPassword"];
        public BusinessVtbBankHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<List<BusinessVtbBankTransactionsItem>> GetTransactionHistory(string account, string cardNumber)
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
                DateTime date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
                DateTime date2 = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
                var checkTime = date.Hour;
                if (checkTime == 0)
                {
                    date = DateTime.Now.AddDays(-1);
                }
                var paramData = new
                {
                    accountNo = cardNumber,
                    accountType = "DDA",
                    cardNo = "",
                    channel = "eFAST",
                    cifno = cacheToken.cifno,
                    currency = "VND",
                    dorcC = "Credit",
                    dorcD = "Debit",
                    endTime = "23:59:59",
                    fromAmount = 0,
                    fromDate = date.ToString("dd/MM/yyyy"),
                    language = "zh",
                    lastRecord = "",
                    newCore = "",
                    pageIndex = 0,
                    pageSize = 100,
                    queryType = "NORMAL",
                    requestId = cacheToken.requestId,
                    screenResolution = cacheToken.screenResolution,
                    searchKey = "",
                    sessionId = cacheToken.sessionId,
                    startTime = "00:00:00",
                    toAmount = 0,
                    toDate = date2.ToString("dd/MM/yyyy"),
                    username = cacheToken.username,
                    version = "1.0"
                };
                var data = paramData.ToJsonString();
                data = DecodeUnicodeString(data);
                var request = new RestRequest()
                    .AddHeader("Connection", "keep-alive")
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Host", "efast.vietinbank.vn")
                    .AddHeader("Origin", "https://efast.vietinbank.vn")
                    .AddHeader("Referer", "https://efast.vietinbank.vn/account/detail")
                    .AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36")
                    .AddParameter("application/json", data, ParameterType.RequestBody);
                var response = await client.ExecutePostAsync(request);
                try
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (!response.Content.IsNullOrEmpty())
                        {
                            var detailResponse = response.Content.FromJsonString<BusinessVtBankTransactionHistoryResponse>();
                            if (detailResponse != null)
                            {
                                if (detailResponse.status.code == "1")
                                {
                                    return detailResponse.transactions;
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
                    NlogLogger.Error("账户：" + account + ",BusinessVtb获取异常：" + ex);
                }
            }
            return null;
        }

        public BusinessVtbBankLoginModel GetSessionId(string account)
        {
            return _redisService.GetRedisValue<BusinessVtbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BusinessVtbBank, account));
        }

        public void SetSessionId(string account, BusinessVtbBankLoginModel sessionid)
        {
            _redisService.AddRedisValue<BusinessVtbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BusinessVtbBank, account), sessionid);
        }

        public void RemoveToken(string account)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BusinessVtbBank, account));
        }

        public string GetLastRefNoKey(string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.BusinessVtbBank, account));
        }

        public void SetLastRefNoKey(string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.BusinessVtbBank, account), refno);
        }

        private string DecodeUnicodeString(string encodedString)
        {
            StringBuilder decodedString = new StringBuilder();
            for (int i = 0; i < encodedString.Length; i++)
            {
                if (encodedString[i] == '\\' && encodedString[i + 1] == 'u')
                {
                    string hexValue = encodedString.Substring(i + 2, 4);
                    int unicodeValue = Convert.ToInt32(hexValue, 16);
                    decodedString.Append((char)unicodeValue);
                    i += 5;
                }
                else
                {
                    decodedString.Append(encodedString[i]);
                }
            }
            return decodedString.ToString();
        }
    }
}
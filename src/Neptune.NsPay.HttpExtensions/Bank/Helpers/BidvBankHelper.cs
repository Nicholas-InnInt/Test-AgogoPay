using Abp.Extensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.RedisExtensions;

//using NewLife;
using RestSharp;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class BidvBankHelper : IBidvBankHelper
    {
        private readonly IRedisService _redisService;
        public BidvBankHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<BidvBankLoginResponse> GetFirstLogin(string card, string userid, string password, string captcha, string captchaToken, string bankApiUrl)
        {
            try
            {
                var url = bankApiUrl + "bidv.php?act=login";
                var client = new RestClient(url);
                var request = new RestRequest()
                    .AddParameter("user", userid)
                    .AddParameter("pass", password)
                    .AddParameter("token", captchaToken)
                    .AddParameter("value", captcha);

                var response = await client.GetAsync<BidvFirstLoginResponse>(request);
                if (response == null) return null;
                if (response.code == 0)
                {
                    BidvBankLoginResponse loginResponse = new BidvBankLoginResponse();
                    loginResponse.loginCache = response;

                    SetSessionId(card, loginResponse);
                    return loginResponse;
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("BidvBank登录解密错误：" + ex);
                return null;
            }
        }

        public async Task<BidvBankLoginResponse> GetSecondLoginAsync(string card, string userid, string token, string bankApiUrl)
        {
            try
            {
                if (!token.IsNullOrEmpty())
                {
                    var url = bankApiUrl + "bidv.php?act=verify";
                    var client = new RestClient(url);
                    var request = new RestRequest()
                        .AddParameter("user", userid)
                        .AddParameter("token", token);

                    var response = await client.GetAsync<BidvSeconLoginResponse>(request);
                    if (response == null) return null;
                    if (response.code == 0)
                    {
                        BidvBankLoginResponse loginResponse = new BidvBankLoginResponse();
                        loginResponse.token = token;
                        loginResponse.Createtime = DateTime.Now;

                        SetSessionId(card, loginResponse);
                        return loginResponse;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("BidvBank登录解密错误：" + ex);
                return null;
            }
        }

        public async Task<BidvBankTransactionHistoryResponse> GetHistoryAsync(string account, string cardNo, string bankApiUrl)
        {
            BidvBankTransactionHistoryResponse bidvBanks = null;
            var historyItems = new List<BidvBankTransactionHistoryItem>();
            var nextRunbal = "";
            var postingDate = "";
            var postingOrder = "0";

            var cacheToken = GetSessionId(cardNo);
            if (cacheToken != null && !cacheToken.token.IsNullOrEmpty())
            {
                TimeSpan timeSpan = DateTime.Now - cacheToken.Createtime;
                if (timeSpan.TotalSeconds > 60)
                {
                    cacheToken.Createtime = DateTime.Now;
                    await GetRefresh(account, bankApiUrl);
                    SetSessionId(cardNo, cacheToken);
                }
                for (var i = 0; i < 5; i++)
                {
                    var list = await GetHistoryByPageAsync(account, cardNo, nextRunbal, postingDate, postingOrder, bankApiUrl);
                    if (list != null)
                    {
                        nextRunbal = list.nextRunbal;
                        postingDate = list.postingDate;
                        postingOrder = list.postingOrder;
                        if (list.data != null && list.data.Count > 0)
                        {
                            historyItems.AddRange(list.data);
                        }
                    }
                }
                if (historyItems.Count > 0)
                {
                    bidvBanks = new BidvBankTransactionHistoryResponse();
                    bidvBanks.code = 0;
                    bidvBanks.data = historyItems;
                }
            }
            return bidvBanks;
        }

        public async Task GetRefresh(string account, string bankApiUrl)
        {
            var url = bankApiUrl + "bidv.php?act=refresh";

            var client = new RestClient(url);
            var request = new RestRequest()
                    .AddParameter("user", account);
            var response = await client.ExecuteGetAsync(request);
            if (response == null) return;
            await Task.CompletedTask;
        }


        public async Task<BidvBankTransactionHistoryResponse> GetHistoryByPageAsync(string account, string cardNo, string nextRunbal, string postingDate, string postingOrder, string bankApiUrl)
        {
            try
            {
                var url = bankApiUrl + "bidv.php?act=record";

                if (nextRunbal.IsNullOrEmpty() && postingDate.IsNullOrEmpty())
                {
                    var client = new RestClient(url);
                    var request = new RestRequest()
                            .AddParameter("user", account)
                            .AddParameter("accountNo", cardNo);
                    var response = await client.GetAsync<BidvBankTransactionHistoryResponse>(request);
                    if (response == null) return null;
                    return response;
                }
                else
                {
                    var client = new RestClient(url);
                    var request = new RestRequest()
                            .AddParameter("user", account)
                            .AddParameter("accountNo", cardNo)
                            .AddParameter("nextRunbal", nextRunbal)
                            .AddParameter("postingDate", postingDate)
                            .AddParameter("postingOrder", postingOrder);
                    var response = await client.GetAsync<BidvBankTransactionHistoryResponse>(request);
                    if (response == null) return null;
                    return response;
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("BidvBank获取订单数据：" + ex);
                return null;
            }
        }


        public BidvBankLoginResponse GetSessionId(string account)
        {
            return _redisService.GetRedisValue<BidvBankLoginResponse>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BidvBank, account));
        }


        public void SetSessionId(string account, BidvBankLoginResponse sessionid)
        {
            _redisService.AddRedisValue<BidvBankLoginResponse>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BidvBank, account), sessionid);
        }

        public void RemoveSessionId(string account)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BidvBank, account));
        }

        public string GetLastRefNoKey(string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.BidvBank, account));
        }

        public void SetLastRefNoKey(string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.BidvBank, account), refno);
        }
    }
}
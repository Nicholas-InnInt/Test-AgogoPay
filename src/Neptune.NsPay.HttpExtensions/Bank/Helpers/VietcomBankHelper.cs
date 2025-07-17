using Abp.Extensions;
using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.RedisExtensions;
using RestSharp;
using System.Globalization;
using System.Text;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class VietcomBankHelper : IVietcomBankHelper
    {
        private readonly IRedisService _redisService;

        public VietcomBankHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<VietcomBankLoginResponse> GetFirstLoginAsync(VietcomLoginRequest input, string cardNo, string bankApiUrl)
        {
            try
            {
                if (input == null) return null;
                if (input.username.IsNullOrEmpty() || input.password.IsNullOrEmpty()) return null;
                if (input.captchaToken.IsNullOrEmpty() || input.captchaValue.IsNullOrEmpty()) return null;
                var url = bankApiUrl + "vietcom.php?act=login";
                var client = new RestClient(url);
                var request = new RestRequest()
                    .AddHeader("Accept", "application/x-www-form-urlencoded")
                    .AddParameter("user", input.username)
                    .AddParameter("pass", input.password)
                    .AddParameter("token", input.captchaToken)
                    .AddParameter("value", input.captchaValue);
                var response = await client.GetAsync<VietcomFirstLoginResponse>(request);

                if (response == null) return null;
                if (response.code == 0)
                {
                    if (response.data != null)
                    {
                        VietcomBankLoginResponse loginResponse = new VietcomBankLoginResponse();
                        loginResponse.loginCache = response.data;
                        ReplaceSessionId(cardNo, loginResponse);
                        return loginResponse;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("VietcomBank登录解密错误：" + ex);
                return null;
            }
        }

        public async Task<VietcomBankLoginResponse> GetSecondLoginAsync(VietcomLoginRequest input, string cardNo, string bankApiUrl)
        {
            try
            {
                if (input == null) return null;
                if (input.codeotp.IsNullOrEmpty() || input.username.IsNullOrEmpty()) return null;

                var cacheToken = GetSessionId(cardNo);
                if (cacheToken != null)
                {
                    if (cacheToken.loginCache != null)
                    {
                        var url = bankApiUrl + "vietcom.php?act=verify";
                        var client = new RestClient(url);
                        var request = new RestRequest()
                            .AddHeader("Accept", "application/x-www-form-urlencoded")
                            .AddParameter("user", input.username)
                            .AddParameter("browserId", cacheToken.loginCache.browserId)
                            .AddParameter("tranId", cacheToken.loginCache.tranId)
                            .AddParameter("browserToken", cacheToken.loginCache.browserToken)
                            .AddParameter("challenge", cacheToken.loginCache.challenge)
                            .AddParameter("otp", input.codeotp);
                        var response = await client.GetAsync<VietcomSecondLoginResponse>(request);
                        if (response == null) return null;
                        if (response.code == 0)
                        {
                            if (response.data != null)
                            {
                                VietcomBankLoginResponse loginResponse = new VietcomBankLoginResponse();
                                loginResponse.code = response.code;
                                loginResponse.userInfo = response.data;
                                loginResponse.loginCache = cacheToken.loginCache;
                                loginResponse.CreateTime = DateTime.Now;
                                ReplaceSessionId(input.username, loginResponse);
                                return loginResponse;
                            }
                        }
                    }
                    return null;
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("VietcomBank登录解密错误：" + ex);
                return null;
            }
        }

        public async Task<ViecomBankTransactionHistoryResponse> GetTransactionHistory(string account, string cardnumber, string bankApiUrl)
        {
            ViecomBankTransactionHistoryResponse viecomBanks = null;
            var historyItems = new List<ViecomBankTransactionHistoryItem>();
            var cacheToken = GetSessionId(cardnumber);
            if (cacheToken != null)
            {
                var flag = 0;
                for (var i = 0; i < 4; i++)
                {
                    var list = await GetTransactionHistoryByPage(account, cardnumber, i, cacheToken, bankApiUrl);
                    if (list == null)
                    {
                        flag += 1;
                    }
                    if (list != null)
                    {
                        if (list.Count == 0)
                        {
                            break;
                        }
                        else
                        {
                            historyItems.AddRange(list);
                        }
                    }
                }
                if (flag < 4)
                {
                    viecomBanks = new ViecomBankTransactionHistoryResponse();
                    viecomBanks.code = 0;
                    viecomBanks.data = historyItems;
                }
            }
            return viecomBanks;
        }

        public async Task<List<ViecomBankTransactionHistoryItem>> GetTransactionHistoryByPage(string account, string cardnumber, int pages, VietcomBankLoginResponse cacheToken, string bankApiUrl)
        {
            try
            {
                var url = bankApiUrl + "vietcom.php?act=record";
                var client = new RestClient(url);
                var request = new RestRequest()
                    .AddParameter("user", account)
                    .AddParameter("accountNo", cardnumber)
                    .AddParameter("pages", pages)
                    .AddParameter("cif", cacheToken.userInfo.cif)
                    .AddParameter("mobileId", cacheToken.userInfo.mobileId)
                    .AddParameter("clientId", cacheToken.userInfo.clientId)
                    .AddParameter("sessionId", cacheToken.userInfo.sessionId)
                    .AddParameter("browserId", cacheToken.loginCache.browserId);
                Random rnd = new Random(Guid.NewGuid().GetHashCode());

                var response = await client.GetAsync<ViecomBankTransactionHistoryResponse>(request);
                if (response == null) return null;

                return response.data;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + account + "VietcomBank获取错误：" + ex);
                return null;
            }
        }

        public async Task<decimal> GetBalance(string account, string cardnumber, string bankApiUrl)
        {
            try
            {
                var cacheToken = GetSessionId(cardnumber);
                if (cacheToken != null)
                {
                    var url = bankApiUrl + "vietcom.php?act=balance";
                    var client = new RestClient(url);
                    var request = new RestRequest()
                        .AddParameter("user", account)
                        .AddParameter("accountNo", cardnumber)
                        .AddParameter("cif", cacheToken.userInfo.cif)
                        .AddParameter("mobileId", cacheToken.userInfo.mobileId)
                        .AddParameter("clientId", cacheToken.userInfo.clientId)
                        .AddParameter("sessionId", cacheToken.userInfo.sessionId)
                        .AddParameter("browserId", cacheToken.loginCache.browserId);
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());

                    var response = await client.GetAsync<VietcomBankBalance>(request);
                    NlogLogger.Info("账户：" + account + "VietcomBank获取余额：" + response.ToJsonString());
                    if (response == null) return 0;
                    if (response.code == 0)
                    {
                        return Convert.ToDecimal(response.balance.Replace(",", ""));
                    }
                    return 0;
                }
                return 0;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + account + "VietcomBank获取余额错误：" + ex);
                return 0;
            }
        }

        public bool Verify(string account)
        {
            var cacheToken = GetSessionId(account);
            if (cacheToken != null)
            {
                if (cacheToken.userInfo == null)
                {
                    return false;
                }
                if (cacheToken.userInfo.mobileId.IsNullOrEmpty())
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public VietcomBankLoginResponse GetSessionId(string account)
        {
            return _redisService.GetRedisValue<VietcomBankLoginResponse>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.VietcomBank, account));
        }

        public void SetSessionId(string account, VietcomBankLoginResponse sessionid)
        {
            _redisService.AddRedisValue<VietcomBankLoginResponse>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.VietcomBank, account), sessionid);
        }

        public void RemoveSessionId(string account)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.VietcomBank, account));
        }

        public void ReplaceSessionId(string account, VietcomBankLoginResponse sessionid)
        {
            _redisService.AddRedisValue<VietcomBankLoginResponse>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.VietcomBank, account), sessionid);
        }

        public string GetLastRefNoKey(string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.VietcomBank, account));
        }

        public void SetLastRefNoKey(string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.VietcomBank, account), refno);
        }

        string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
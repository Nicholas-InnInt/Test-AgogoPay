using Abp.Extensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.RedisExtensions;
using RestSharp;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class AcbBankHelper : IAcbBankHelper
    {
        private readonly IRedisService _redisService;

        public AcbBankHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<AcbBankTransactionHistoryResponse> GetHistoryListAsync(string account, string cardno, string bankApiUrl, int page, int totalpage, int type)
        {
            //获取记录不需要在登录,增加错误机制，查询超过5次错误在删除
            try
            {
                var cacheToken = GetSessionId(cardno);
                if (cacheToken != null)
                {
                    var url = bankApiUrl + "acb.php?act=record";
                    if (type == 1)
                    {
                        url = bankApiUrl + "businessacb.php?act=record";
                    }
                    var client = new RestClient(url);
                    var request = new RestRequest()
                            .AddParameter("user", account)
                            .AddParameter("account", cardno);

                    if (type == 1)
                    {
                        request.AddParameter("cookie", cacheToken.Cookies);
                        request.AddParameter("page", page);
                        request.AddParameter("totalPage", totalpage);
                    }
                    var response = await client.ExecuteGetAsync(request);
                    if (type == 1)
                    {
                        NlogLogger.Warn("账户:" + account + ",Api:" + bankApiUrl + "AcbBank获取订单数据：" + response.Content);
                    }
                    if (response == null)
                    {
                        return null;
                    }
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var detailResponse = JsonConvert.DeserializeObject<AcbBankTransactionHistoryResponse>(response.Content);
                        if (detailResponse != null)
                        {
                            if (detailResponse.code == 0)
                            {

                                return detailResponse;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户:" + account + ",Api:" + bankApiUrl + "AcbBank获取订单数据：" + ex);
                return null;
            }
        }

        public async Task<List<AcbBankTransactionHistoryData>> GetHistoryAsync(string account, string cardno, string bankApiUrl, int type)
        {
            var cacheToken = GetSessionId(cardno);
            if (cacheToken != null)
            {
                TimeSpan timeSpan = DateTime.Now - cacheToken.CreateTime;

                if (type == 0)
                {
                    if (timeSpan.TotalSeconds > 50)
                    {
                        cacheToken.CreateTime = DateTime.Now;
                        await GetRefresh(account, bankApiUrl, type);
                    }
                }

                List<AcbBankTransactionHistoryData> acbBanks = new List<AcbBankTransactionHistoryData>();
                if (type == 1)
                {
                    var totalPage = cacheToken.CurrentPage;
                    if (totalPage == 0)
                    {
                        totalPage = 1;
                    }
                    //if (cacheToken.UpdateTime.HasValue)
                    //{
                    //    var cachetime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(cacheToken.UpdateTime.Value, "SE Asia Standard Time");
                    //    var time = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");

                    //    if (cachetime.Day != time.Day)
                    //    {
                    //        totalPage = 1;
                    //        cacheToken.UpdateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
                    //    }
                    //}
                    var page = totalPage;
                    var response = await GetHistoryListAsync(account, cardno, bankApiUrl, page, totalPage, type);
                    if (response != null)
                    {
                        if (response.code == 0)
                        {
                            if (response.data.Count == 501)
                            {
                                acbBanks.AddRange(response.data);
                                totalPage = totalPage + 1;
                                page = page + 1;
                                response = await GetHistoryListAsync(account, cardno, bankApiUrl, page, totalPage, type);
                                if (response != null)
                                {
                                    if (response.data.Count != 501 && response.code == 0)
                                    {
                                        cacheToken.CurrentPage = totalPage;
                                        acbBanks.AddRange(response.data);
                                    }
                                    else
                                    {
                                        if (response.data.Count > 0)
                                        {
                                            cacheToken.CurrentPage = totalPage;
                                            acbBanks.AddRange(response.data);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (response.data.Count > 0)
                                {
                                    cacheToken.CurrentPage = totalPage;
                                    acbBanks.AddRange(response.data);
                                    //break;
                                }
                            }
                        }
                        else
                        {
                            totalPage = 3;
                            cacheToken.ErrorCount = 0;
                            acbBanks.AddRange(response.data);
                        }
                    }
                    else
                    {
                        cacheToken.ErrorCount = cacheToken.ErrorCount + 1;
                    }
                    //}
                    if (!cacheToken.UpdateTime.HasValue)
                    {
                        cacheToken.UpdateTime = DateTime.Now;
                    }
                    cacheToken.TotalPage = totalPage;
                    ReplaceSessionId(cardno, cacheToken);
                }
                else
                {
                    var response = await GetHistoryListAsync(account, cardno, bankApiUrl, 0, 0, type);
                    if (response != null)
                    {
                        if (response.code == 0 && response.data.Count == 0)
                        {
                            for (var i = 0; i < 5; i++)
                            {
                                //刷新
                                await GetRefresh(account, bankApiUrl, type);
                                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                                Thread.Sleep(1000 * rnd.Next(3));
                                response = await GetHistoryListAsync(account, cardno, bankApiUrl, 0, 0, type);
                                if (response != null)
                                {
                                    if (response.code == 0 && response.data.Count >= 0)
                                    {
                                        acbBanks.AddRange(response.data);
                                        break;
                                    }
                                }
                            }
                        }
                        cacheToken.ErrorCount = 0;
                        acbBanks.AddRange(response.data);
                    }
                    else
                    {
                        cacheToken.ErrorCount = cacheToken.ErrorCount + 1;
                    }

                    ReplaceSessionId(cardno, cacheToken);
                }
                return acbBanks;
            }
            return null;
        }
        public async Task<List<AcbBankTransactionHistoryData>> GetBusinessHistoryAsync(string account, string cardno, string bankApiUrl, int type)
        {
            var cacheToken = GetSessionId(cardno);
            if (cacheToken != null)
            {
                List<AcbBankTransactionHistoryData> acbBanks = new List<AcbBankTransactionHistoryData>();
                if (type == 1)
                {
                    var totalPage = cacheToken.CurrentPage;
                    if (totalPage == 0)
                    {
                        totalPage = 1;
                    }

                    //记录抓取第一次时间，如果超过越南时间0点，重置页数
                    var updateTime = GetBusinessUpdateTime(cardno);
                    if (updateTime.IsNullOrEmpty())
                    {
                        updateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        SetBusinessUpdateTime(cardno, updateTime);
                    }
                    var strartTime = updateTime.ParseToDateTime();
                    var dayTime = DateTime.Now;
                    DateTime time = new DateTime(dayTime.Year, dayTime.Month, dayTime.Day, 01, 00, 00);
                    TimeSpan timeSpan = time - strartTime;
                    if (timeSpan.TotalMinutes > 3 && dayTime.Hour == 1)
                    {
                        //重置
                        SetBusinessUpdateTime(cardno, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        totalPage = 1;
                    }

                    var page = totalPage;
                    var response = await GetHistoryListAsync(account, cardno, bankApiUrl, page, totalPage, type);
                    if (response != null)
                    {
                        var result = CheckBusinessApiResult(response);
                        if (result == false)
                        {
                            for (var i = 0; i < 2; i++)
                            {
                                response = await GetHistoryListAsync(account, cardno, bankApiUrl, page, totalPage, type);
                                result = CheckBusinessApiResult(response);
                                if (result)
                                {
                                    break;
                                }
                            }
                        }
                        var resultNew = CheckBusinessApiNewResult(response);
                        if (resultNew == false)
                        {
                            for (var i = 0; i < 2; i++)
                            {
                                response = await GetHistoryListAsync(account, cardno, bankApiUrl, page, totalPage, type);
                                resultNew = CheckBusinessApiResult(response);
                                if (result)
                                {
                                    break;
                                }
                            }
                        }
                        if (result && resultNew)
                        {
                            if (response.code == 0)
                            {
                                if (response.data.Count == 501)
                                {
                                    acbBanks.AddRange(response.data);
                                    totalPage = totalPage + 1;
                                    page = page + 1;
                                    response = await GetHistoryListAsync(account, cardno, bankApiUrl, page, totalPage, type);
                                    if (response != null)
                                    {
                                        var result2 = CheckBusinessApiResult(response);
                                        if (result2 == false)
                                        {
                                            for (var i = 0; i < 2; i++)
                                            {
                                                response = await GetHistoryListAsync(account, cardno, bankApiUrl, page, totalPage, type);
                                                result2 = CheckBusinessApiResult(response);
                                                if (result2)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        var resultNew2 = CheckBusinessApiNewResult(response);
                                        if (resultNew2 == false)
                                        {
                                            for (var i = 0; i < 2; i++)
                                            {
                                                response = await GetHistoryListAsync(account, cardno, bankApiUrl, page, totalPage, type);
                                                resultNew2 = CheckBusinessApiNewResult(response);
                                                if (resultNew2)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        if (result2 && resultNew2)
                                        {
                                            if (response.data.Count != 501 && response.code == 0)
                                            {
                                                cacheToken.CurrentPage = totalPage;
                                                acbBanks.AddRange(response.data);
                                            }
                                            else
                                            {
                                                if (response.data.Count > 0)
                                                {
                                                    cacheToken.CurrentPage = totalPage;
                                                    acbBanks.AddRange(response.data);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (response.data.Count > 0)
                                    {
                                        cacheToken.CurrentPage = totalPage;
                                        acbBanks.AddRange(response.data);
                                        //break;
                                    }
                                }
                            }
                            else
                            {
                                totalPage = 3;
                                cacheToken.ErrorCount = 0;
                                acbBanks.AddRange(response.data);
                            }
                        }
                    }
                    else
                    {
                        cacheToken.ErrorCount = cacheToken.ErrorCount + 1;
                    }
                    if (!cacheToken.UpdateTime.HasValue)
                    {
                        cacheToken.UpdateTime = DateTime.Now;
                    }
                    cacheToken.TotalPage = totalPage;
                    ReplaceSessionId(cardno, cacheToken);
                }
                return acbBanks;
            }
            return null;
        }

        private bool CheckBusinessApiResult(AcbBankTransactionHistoryResponse response)
        {
            if (response == null)
            {
                return false;
            }
            if (response.code == 0 && response.balance == "0" && response.data.Count == 0)
            {
                return false;
            }
            return true;
        }

        private bool CheckBusinessApiNewResult(AcbBankTransactionHistoryResponse response)
        {
            if (response.code == 0 && response.data.Count == 0 && response.totalPage > 0)
            {
                return false;
            }
            return true;
        }



        public async Task<AcbBankRefreshResponse> GetRefresh(string account, string bankApiUrl, int type)
        {
            try
            {
                var url = bankApiUrl + "acb.php?act=refresh";
                if (type == 1)
                {
                    url = bankApiUrl + "businessacb.php?act=refresh";
                }
                var client = new RestClient(url);
                var request = new RestRequest()
                        .AddParameter("user", account);

                var response = await client.GetAsync<AcbBankRefreshResponse>(request);
                if (response != null)
                {
                    if (response.code == 0)
                    {
                        return response;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("AcbBank刷新任务错误：" + ex);
                return null;
            }
        }

        public async Task<AcbBankTransactionHistoryResponse> GetLoginAsync(string account, string cardno, string cookie, string bankApiUrl, int type)
        {
            //第一次请求使用cookie
            try
            {
                var url = bankApiUrl + "acb.php?act=record";
                if (type == 1)
                {
                    url = bankApiUrl + "businessacb.php?act=record";
                }

                var client = new RestClient(url);
                var request = new RestRequest()
                        .AddParameter("user", account)
                        .AddParameter("account", cardno)
                        .AddParameter("cookie", cookie);
                if (type == 1)
                {
                    request.AddParameter("page", 1);
                    request.AddParameter("totalPage", 0);
                }
                var response = await client.ExecuteAsync(request);
                if (response == null) return null;
                var detailResponse = JsonConvert.DeserializeObject<AcbBankTransactionHistoryResponse>(response.Content);
                if (detailResponse.code == 0)
                {
                    //RemoveSessionId(cardno);
                    var totalPage = 1;
                    //if (type == 1)
                    //{
                    //    totalPage = response.totalPage;
                    //}
                    AcbBankLoginModel model = new AcbBankLoginModel()
                    {
                        Cookies = cookie,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                        ErrorCount = 0,
                        TotalPage = totalPage,
                        CurrentPage = 1,
                    };
                    var cacheToken = GetSessionId(cardno);
                    if (cacheToken != null)
                    {
                        var currentPage = cacheToken.CurrentPage;
                        if (cacheToken.UpdateTime.HasValue)
                        {
                            TimeSpan timeSpan = cacheToken.UpdateTime.Value - DateTime.Now;
                            if (timeSpan.Hours > 24)
                            {
                                currentPage = 1;
                            }
                        }
                        else
                        {
                            cacheToken.UpdateTime = DateTime.Now;
                        }
                        cacheToken.CurrentPage = currentPage;
                        cacheToken.Cookies = cookie;
                        cacheToken.CreateTime = DateTime.Now;
                        cacheToken.ErrorCount = 0;
                        cacheToken.UpdateTime = cacheToken.UpdateTime;
                        SetSessionId(cardno, cacheToken);
                    }
                    else
                    {
                        SetSessionId(cardno, model);
                    }
                    return detailResponse;
                }
                return null;

            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户" + account + ",AcbBank登录错误：" + ex);
                return null;
            }
        }


        public AcbBankLoginModel GetSessionId(string account)
        {
            return _redisService.GetRedisValue<AcbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.ACBBank, account));
        }

        public void SetSessionId(string account, AcbBankLoginModel sessionid)
        {
            _redisService.AddRedisValue<AcbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.ACBBank, account), sessionid);
        }

        public void RemoveSessionId(string account)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.ACBBank, account));
        }

        public void ReplaceSessionId(string account, AcbBankLoginModel sessionid)
        {
            _redisService.AddRedisValue<AcbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.ACBBank, account), sessionid);
        }

        public string GetLastRefNoKey(string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.ACBBank, account));
        }

        public void SetLastRefNoKey(string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.ACBBank, account), refno);
        }

        public string GetBusinessUpdateTime(string cardNo)
        {
            return _redisService.GetRedisValue<string>("BusinessAcbTime:UpdateTime_" + cardNo);
        }

        public void SetBusinessUpdateTime(string cardNo, string updateTime)
        {
            _redisService.AddRedisValue<string>("BusinessAcbTime:UpdateTime_" + cardNo, updateTime);
        }
    }
}
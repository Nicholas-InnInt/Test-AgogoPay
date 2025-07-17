using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.RedisExtensions;
using RestSharp;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class PVcomBankHelper : IPVcomBankHelper
    {
        private readonly IRedisService _redisService;

        public PVcomBankHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<bool> Verify(string cardno)
        {
            var cacheToken = GetSessionId(cardno);
            if (cacheToken != null)
            {
                TimeSpan timeSpan = DateTime.Now - cacheToken.CreateTime;
                if (timeSpan.TotalMinutes > 2.5)
                {
                    var result = await RefreshTokenAsync(cardno);
                    if (result != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public async Task<PVcomBankLoginModel> RefreshTokenAsync(string cardno)
        {
            try
            {
                var cacheToken = GetSessionId(cardno);
                if (cacheToken != null)
                {
                    var url = "https://connect.pvcombank.com.vn/auth/realms/pvcombank/protocol/openid-connect/token";
                    var client = new RestClient(url);
                    var request = new RestRequest()
                            .AddHeader("Content-Type", "application/x-www-form-urlencoded")
                            .AddParameter("grant_type", "refresh_token")
                            .AddParameter("refresh_token", cacheToken.refresh_token)
                            .AddParameter("client_id", "mbank");
                    var response = await client.PostAsync<PVcomBankLoginModel>(request);
                    if (response == null) return null;

                    response.cookie = cacheToken.cookie;
                    response.CreateTime = DateTime.Now;
                    SetSessionId(cardno, response);
                    return response;
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户" + cardno + "PVcomBank刷新任务错误：" + ex);
                return null;
            }
        }

        public async Task<List<PVcomBankTransactionHistoryList>> GetHistoryAsync(string account, string cardNo)
        {
            try
            {
                var cacheToken = GetSessionId(cardNo);
                if (cacheToken != null)
                {
                    var userAccount = await GetQueryAccountList(account, cardNo);
                    if (userAccount == null) return null;
                    var dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
                    DateTime date = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
                    var checkTime = dateTime.Hour;
                    if (checkTime == 0)
                    {
                        date = DateTime.Now.AddDays(-1);
                    }
                    var sessionId = userAccount.HEADER.SESSION_ID;
                    var url = "https://eb.cloud-sg.pvcombank.com.vn/api/eb/system/v2/user/getIbStatementV2";
                    var requestData = new
                    {
                        HEADER = new
                        {
                            ACTION = "Y",
                            MERCHANTID = userAccount.HEADER.MERCHANTID,
                            MESSAGEID = "GET_IB_STATEMENT_V2",
                            CHANNEL = "IB",
                            LANGUAGE = "VI",
                            SYSTEMID = userAccount.HEADER.SYSTEMID,
                            SESSION_ID = sessionId,
                            SYSTEM_DATE = dateTime.ToString("dd/MM/yyyy hh:mm:ss"),
                            HASKEY = userAccount.HEADER.HASKEY,
                        },
                        REQUESTINPUT = new
                        {
                            USERPROFILE = new
                            {
                                SESSONDATE = dateTime.ToString("dd/MM/yyyy hh:mm:ss"),
                                SESSIONID = sessionId,
                                USERGROUPTYPE = userAccount.REQUESTINPUT.USERPROFILE.USERGROUPTYPE,
                                CIFNUMBER = userAccount.REQUESTINPUT.USERPROFILE.CIFNUMBER
                            },
                            ACCOUNTPROFILE = new
                            {
                                ACCOUNTNO = cardNo,
                                ACCOUNTTYPE = "10"
                            },
                            STATEMENT = new
                            {
                                FROMDATE = date.ToString("yyyyMMdd"),
                                TODATE = dateTime.ToString("yyyyMMdd"),
                            }
                        }
                    };
                    var postdata = requestData.ToJsonString();
                    var client = new RestClient(url);
                    var request = new RestRequest()
                        .AddHeader("Authorization", "Bearer " + cacheToken.access_token)
                        .AddParameter("application/json", postdata, ParameterType.RequestBody);
                    var response = await client.ExecutePostAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var detailResponse = response.Content.FromJsonString<PVcomBankTransactionHistoryResponse>();
                        if (detailResponse != null)
                        {
                            if (detailResponse.RESULT.CODE.ToLower() == "e00".ToLower())
                            {
                                if (detailResponse.RESPONSEDATA.STATEMENT != null)
                                {
                                    return detailResponse.RESPONSEDATA.STATEMENT.LIST_STATEMENT_360;
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + account + ",PVcomBank获取数据错误：" + ex);
                return null;
            }
        }

        public async Task<PVcomBankAccountResponse> GetQueryAccountList(string account, string cardno)
        {
            try
            {
                var cacheToken = GetSessionId(cardno);
                if (cacheToken != null)
                {
                    var dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
                    var sessionid = cacheToken.session_state.Replace("-", "");
                    var url = "https://eb.cloud-sg.pvcombank.com.vn/api/eb/system/v2/user/queryAccountList";
                    var client = new RestClient(url);
                    var requestData = new
                    {
                        HEADER = new
                        {
                            ACTION = "N",
                            CHANNEL = "IB",
                            LANGUAGE = "VI",
                            MERCHANTID = "10101",
                            MESSAGEID = "QUERY_ACCOUNT_LIST",
                            SESSION_ID = sessionid,
                            SYSTEMID = "E85A008768CCF2820549233FD8DE4904",
                            SYSTEM_DATE = dateTime.ToString("dd/MM/yyyy hh:mm:ss"),
                        },
                        REQUESTINPUT = new
                        {
                            USERPROFILE = new
                            {
                                SESSONDATE = dateTime.ToString("dd/MM/yyyy hh:mm:ss"),
                                SESSIONID = sessionid,
                                USERGROUPTYPE = "01"
                            }
                        }
                    };
                    var postdata = requestData.ToJsonString();
                    var request = new RestRequest()
                        .AddHeader("Authorization", "Bearer " + cacheToken.access_token)
                        .AddParameter("application/json", postdata, ParameterType.RequestBody);
                    var response = await client.ExecutePostAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var detailResponse = response.Content.FromJsonString<PVcomBankAccountResponse>();
                        if (detailResponse != null)
                        {
                            return detailResponse;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + account + ",PVcomBank获取个人信息错误：" + ex);
                return null;
            }
        }

        public PVcomBankLoginModel GetSessionId(string account)
        {
            return _redisService.GetRedisValue<PVcomBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.PVcomBank, account));
        }

        public void SetSessionId(string account, PVcomBankLoginModel sessionid)
        {
            _redisService.AddRedisValue<PVcomBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.PVcomBank, account), sessionid);
        }

        public void RemoveSessionId(string account)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.PVcomBank, account));
        }

        public string GetLastRefNoKey(string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.PVcomBank, account));
        }

        public void SetLastRefNoKey(string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.PVcomBank, account), refno);
        }
    }
}
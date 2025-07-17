using Neptune.NsPay.Commons;
using Neptune.NsPay.PlatformWithdraw.Models;
using Neptune.NsPay.RedisExtensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.PlatformWithdraw.Helper
{
    public class PlatformWebHelper
    {
        private readonly IRedisService _cache;
        string account;
        string password;
        string webUrl;
        public PlatformWebHelper(IRedisService redisCache)
        {
            webUrl = AppSettings.Configuration["PlatformUrl"];
            account = AppSettings.Configuration["Account"];
            password = AppSettings.Configuration["PassWord"];
            _cache = redisCache;
        }

        #region 登录

        public PlatfromLoginResult? Login()
        {
            string url = webUrl + "api/2.0/account/login";
            var pwd = password;
            var request = new
            {
                account = account,
                password = pwd,
            };
            var data = JsonConvert.SerializeObject(request);
            var response = HttpHelper.ReqUrl(url, "POST", data, "");
            NlogLogger.Warn("账户：" + account + "登录返回：" + response);
            if (response == null || string.IsNullOrEmpty(response))
            {
                return null;
            }
            var detailResponse = JsonConvert.DeserializeObject<PlatfromLoginResponse>(response);
            if (detailResponse == null)
            {
                return null;
            }
            string cacheKey = GetCacheKey(account);
            if (detailResponse.Code == 200)
            {
                detailResponse.Result.loginTime = DateTime.Now;
                if (string.IsNullOrEmpty(cacheKey))
                {
                    SetCacheKey(account, detailResponse.Result);
                }
                else
                {
                    RemoveCacheKey(account);//移除在添加
                    SetCacheKey(account, detailResponse.Result);
                }
            }
            return detailResponse.Result;
        }

        public PlatfromLoginResult Verify()
        {
            string cacheKey = GetCacheKey(account);
            if (!string.IsNullOrEmpty(cacheKey))
            {
                PlatfromLoginResult extra = JsonConvert.DeserializeObject<PlatfromLoginResult>(cacheKey);
                TimeSpan timeSpan = DateTime.Now - extra.loginTime;
                if (timeSpan.TotalMinutes > 15)
                {
                    return Login();
                }
                return extra;
            }
            else
            {
                return Login();
            }
        }

        #endregion

        #region 出款管理

        //获取出款记录
        public List<GetVerifyWithdrawData> GetVerifyWithdrawDataList(string levels, string memo)
        {
            string cacheKey = GetCacheKey(account);
            if (!string.IsNullOrEmpty(cacheKey))
            {
                PlatfromLoginResult extra = JsonConvert.DeserializeObject<PlatfromLoginResult>(cacheKey);
                if (!string.IsNullOrEmpty(extra.AccessToken))
                {
                    string url = webUrl + "api/1.0/verifyWithdraw/list";
                    var data = "";
                    if (string.IsNullOrEmpty(memo))
                    {
                        data = "{\"count\":100,\"query\":{\"#\":null,\"isCheckStates\":true,\"memberLevels\":[" + levels + "],notLock: true,\"states\":\"1\"}}";
                    }
                    else
                    {
                        data = "{\"count\":100,\"query\":{\"#\":null,\"isCheckStates\":true,\"memberLevels\":[" + levels + "],\"memo\":\"" + memo + "\",notLock: true,\"states\":\"1\"}}";
                    }
                    var response = HttpHelper.ReqUrl(url, "POST", data, extra.AccessToken);
                    NlogLogger.Warn("账户：" + account + ",出款数据返回：" + response);
                    if (response == null || string.IsNullOrEmpty(response))
                    {
                        return null;
                    }
                    var detailResponse = JsonConvert.DeserializeObject<GetVerifyWithdrawReponse>(response);
                    if (detailResponse == null)
                    {
                        return null;
                    }
                    if (detailResponse.Code == 200)
                    {
                        if (detailResponse.Result != null)
                        {
                            return detailResponse.Result.Data;
                        }
                    }

                }
            }
            return null;
        }

        //获取出款详情
        public GetWithdrawResult GetWithdrawDetail(long id)
        {
            string cacheKey = GetCacheKey(account);
            if (!string.IsNullOrEmpty(cacheKey))
            {
                PlatfromLoginResult extra = JsonConvert.DeserializeObject<PlatfromLoginResult>(cacheKey);
                if (!string.IsNullOrEmpty(extra.AccessToken))
                {
                    string tokenUrl = webUrl + "signalr/negotiate?clientProtocol=2.1&connectionData=%5B%7B%22name%22%3A%22mainhub%22%7D%5D&_=1671356491576";
                    var tokenStr = HttpHelper.ReqUrl(tokenUrl, "GET", "", extra.AccessToken);
                    if (tokenStr == null || string.IsNullOrEmpty(tokenStr))
                    {
                        return null;
                    }
                    var tokenReponse = JsonConvert.DeserializeObject<GetNegotiateConnect>(tokenStr);
                    if (tokenReponse == null)
                    {
                        return null;
                    }

                    var url = webUrl + "api/1.0/verifyWithdraw/detail";
                    var request = new
                    {
                        ApplyId = id,
                        connectionId = tokenReponse.ConnectionId
                    };
                    var data = JsonConvert.SerializeObject(request);
                    var response = HttpHelper.ReqUrl(url, "POST", data, extra.AccessToken);
                    if (response == null || string.IsNullOrEmpty(response))
                    {
                        return null;
                    }
                    var detailResponse = JsonConvert.DeserializeObject<GetWithdrawReponse>(response);
                    if (detailResponse == null)
                    {
                        return null;
                    }
                    if (detailResponse.Code == 200)
                    {
                        if (detailResponse.Result != null)
                        {
                            return detailResponse.Result;
                        }
                    }
                }
            }
            return null;
        }

        //锁定订单
        public bool LockWithdraw(long id)
        {
            string cacheKey = GetCacheKey(account);
            if (!string.IsNullOrEmpty(cacheKey))
            {
                PlatfromLoginResult extra = JsonConvert.DeserializeObject<PlatfromLoginResult>(cacheKey);
                if (!string.IsNullOrEmpty(extra.AccessToken))
                {
                    var url = webUrl + "api/1.0/verifyWithdraw/lock";
                    var requestData = new
                    {
                        applicationId = id
                    };
                    var data = JsonConvert.SerializeObject(requestData);
                    var response = HttpHelper.ReqUrl(url, "POST", data, extra.AccessToken);
                    if (response == null || string.IsNullOrEmpty(response))
                    {
                        return false;
                    }
                    var detailResponse = JsonConvert.DeserializeObject<PlatfromBaseResponse>(response);
                    if (detailResponse == null)
                    {
                        return false;
                    }
                    if (detailResponse.Code == 200)
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        //更新备注
        public bool UpdateMemo(long id, string memo)
        {
            string cacheKey = GetCacheKey(account);
            if (!string.IsNullOrEmpty(cacheKey))
            {
                PlatfromLoginResult extra = JsonConvert.DeserializeObject<PlatfromLoginResult>(cacheKey);
                if (!string.IsNullOrEmpty(extra.AccessToken))
                {
                    var url = webUrl + "VerifyWithdraw/UpdateMemo";
                    var requestData = new
                    {
                        id = id,
                        memo = memo
                    };
                    var data = JsonConvert.SerializeObject(requestData);
                    var response = HttpHelper.ReqUrl(url, "POST", data, extra.AccessToken);
                    if (response == null || string.IsNullOrEmpty(response))
                    {
                        return false;
                    }
                    var detailResponse = Convert.ToBoolean(response);
                    return detailResponse;

                }
            }
            return false;
        }

        public bool VerifyWithdrawAllow(long id)
        {
            string cacheKey = GetCacheKey(account);
            if (!string.IsNullOrEmpty(cacheKey))
            {
                PlatfromLoginResult extra = JsonConvert.DeserializeObject<PlatfromLoginResult>(cacheKey);
                if (!string.IsNullOrEmpty(extra.AccessToken))
                {
                    var url = webUrl + "api/1.0/verifyWithdraw/allow";
                    var requestData = new
                    {
                        applicationId = id
                    };
                    var data = JsonConvert.SerializeObject(requestData);
                    var response = HttpHelper.ReqUrl(url, "POST", data, extra.AccessToken);
                    if (response == null || string.IsNullOrEmpty(response))
                    {
                        return false;
                    }
                    var detailResponse = JsonConvert.DeserializeObject<PlatfromBaseResponse>(response);
                    if (detailResponse == null)
                    {
                        return false;
                    }
                    if (detailResponse.Code == 200)
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        #endregion

        private string GetCacheKey(string account)
        {
            return _cache.GetRedisValue<string>("Platform_" + account);
        }

        private void SetCacheKey(string account, PlatfromLoginResult data)
        {
            _cache.AddRedisValue<PlatfromLoginResult>("Platform_" + account, data);
        }

        private void RemoveCacheKey(string account)
        {
            _cache.RemoveRedisValue("Platform_" + account);
        }

    }
}

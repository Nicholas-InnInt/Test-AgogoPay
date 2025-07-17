using Abp.Extensions;
using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.HttpExtensions.EncryptionHelper;
using Neptune.NsPay.RedisExtensions;
using RestSharp;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    public class VietinBankHelper : IVietinBankHelper
    {
        private readonly static string DoLoginUrl = "https://api-ipay.vietinbank.vn/ipay/wa/signIn";
        private readonly static string TransactionHistoryUrl = "https://api-ipay.vietinbank.vn/ipay/wa/getHistTransactions";


        private readonly static string socksUrl = AppSettings.Configuration["SocksUrl"];
        private readonly static int socksPort = AppSettings.Configuration["SocksPort"].ToInt();
        private readonly static string socksusername = AppSettings.Configuration["SocksUsername"];
        private readonly static string sockspassword = AppSettings.Configuration["SocksPassword"];

        private readonly IRedisService _redisService;

        public VietinBankHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }
        public string GetLogin(string userid, string password, string captchaCode, string captchaId)
        {
            var username = userid;

            var clientinfo = "59.153.218.108;Windows-10";
            var browserInfo = "Chrome-114";
            var requestid = GetRandomStr().ToUpper() + "|" + TimeHelper.GetUnixTimeStamp(DateTime.Now);

            VietinBankLoginRequest loginRequest = new VietinBankLoginRequest()
            {
                accessCode = password,
                browserInfo = browserInfo,
                captchaCode = captchaCode,
                captchaId = captchaId,
                clientInfo = clientinfo,
                requestId = requestid,
                userName = username
            };
            var sign = MD5Helper.MD5Encrypt32(ToPaeameter(loginRequest)).ToLower();
            loginRequest.signature = sign;
            string loginstr = "{\"userName\":\"" + loginRequest.userName + "\",\"accessCode\":\"" + loginRequest.accessCode + "\",\"captchaCode\":\"" + loginRequest.captchaCode + "\",\"captchaId\":\"" + loginRequest.captchaId + "\",\"clientInfo\":\"" + loginRequest.clientInfo + "\",\"browserInfo\":\"" + loginRequest.browserInfo + "\",\"lang\":\"" + loginRequest.lang + "\",\"requestId\":\"" + loginRequest.requestId + "\",\"signature\":\"" + loginRequest.signature + "\"}";
            NlogLogger.Warn("账户：" + userid + "，VTB登录参数：" + loginstr);
            RSACryption cryption = new RSACryption();
            var encrypt = cryption.VietinBankRSAEncrypt(loginstr);
            VietinBankLoginEncryptedRequest vietinBankLogin = new VietinBankLoginEncryptedRequest()
            {
                encrypted = encrypt
            };
            var data = vietinBankLogin.ToJsonString();

            #region 代理

            WebProxy webProxy = new WebProxy(socksUrl, socksPort);
            webProxy.BypassProxyOnLocal = true;
            webProxy.Credentials = new NetworkCredential(socksusername, sockspassword);
            var client = new RestClient(DoLoginUrl);
            if (!socksUrl.IsNullOrEmpty())
            {
                client = new RestClient(new RestClientOptions(DoLoginUrl)
                {
                    Proxy = webProxy
                });
            }
            var request = new RestRequest()
                   .AddParameter("application/json", data, ParameterType.RequestBody);
            var response = client.ExecutePost(request);

            #endregion

            NlogLogger.Warn("账户：" + userid + "，登录返回：" + response.ToJsonString());
            if (response.Content.IsNullOrEmpty())
            {
                return string.Empty;
            }
            var loginResponse = response.Content.FromJsonString<VietinBankLoginResponse>();
            if (loginResponse == null || loginResponse.error == true)
                return string.Empty;
            return loginResponse.sessionId;
        }


        public async Task<List<HisTransactionItem>> GetHistTransactions(string account, string cardNumber)
        {
            string sessionid = GetSessionId(cardNumber);
            if (sessionid.IsNullOrEmpty())
            {
                return null;
            }
            sessionid = sessionid.Replace("\"", "");
            DateTime date = DateTime.Now.AddDays(-1);
            var requestid = GetRandomStr().ToUpper() + "|" + TimeHelper.GetUnixTimeStamp(DateTime.Now);
            var request = new HistTransactionsRequest()
            {
                accountNumber = cardNumber,
                startDate = date.ToString("yyyy-MM-dd"),
                endDate = DateTime.Now.ToString("yyyy-MM-dd"),
                requestId = requestid,
                sessionId = sessionid
            };
            string md5Str = "accountNumber=" + request.accountNumber + "&endDate=" + request.endDate + "&lang=" + request.lang + "&maxResult=" + request.maxResult + "&pageNumber=" + request.pageNumber + "&requestId=" + UrlEncode(request.requestId) + "&searchFromAmt=&searchKey=&searchToAmt=&sessionId=" + request.sessionId + "&startDate=" + request.startDate + "&tranType=";
            var sign = MD5Helper.MD5Encrypt32(md5Str).ToLower();
            request.signature = sign;
            string transctionStr = "{\"accountNumber\":\"" + request.accountNumber + "\",\"endDate\":\"" + request.endDate + "\",\"lang\":\"" + request.lang + "\",\"maxResult\":\"" + request.maxResult + "\",\"pageNumber\":" + request.pageNumber + ",\"requestId\":\"" + request.requestId + "\",\"searchFromAmt\":\"\",\"searchKey\":\"\",\"searchToAmt\":\"\",\"sessionId\":\"" + request.sessionId + "\",\"signature\":\"" + request.signature + "\",\"startDate\":\"" + request.startDate + "\",\"tranType\":\"\"}";
            RSACryption cryption = new RSACryption();
            var encrypt = cryption.VietinBankRSAEncrypt(transctionStr);
            var vietinBankLogin = new VietinBankLoginEncryptedRequest()
            {
                encrypted = encrypt
            };
            var data = vietinBankLogin.ToJsonString();
            IDictionary<string, string> _headers = new Dictionary<string, string>();

            #region 代理

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

            #endregion
            var requestData = new RestRequest()
                    .AddParameter("application/json", data, ParameterType.RequestBody);
            var response = await client.ExecutePostAsync(requestData);

            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (!response.Content.IsNullOrEmpty())
                    {
                        NlogLogger.Info("VietinBank账户：" + account + "订单详情：" + response.Content);
                        var detailResponse = response.Content.FromJsonString<HistTransactionResponse>();
                        if (detailResponse != null)
                        {
                            return detailResponse.transactions;
                        }
                    }
                }
                else
                {
                    NlogLogger.Error("账户：" + account + "VietinBank获取错误：" + response.ToJsonString());
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + account + "VietinBank获取异常：" + ex);
            }
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.VietinBank, account));
            return null;
        }

        public string GetSessionId(string account)
        {
            var value = _redisService.GetRedisValue<VtbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.VietinBank, account));
            return value.sessionid.ToString();
        }

        public VtbBankLoginModel GetVtbBankLoginModel(string account)
        {
            return _redisService.GetRedisValue<VtbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.VietinBank, account));
        }

        public void SetSessionId(string account, VtbBankLoginModel sessionid)
        {
            _redisService.AddRedisValue<VtbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.VietinBank, account), sessionid);
        }

        public void RemoveSessionId(string account)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.VietinBank, account));
        }

        public string GetLastRefNoKey(string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.VietinBank, account));
        }

        public void SetLastRefNoKey(string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.VietinBank, account), refno);
        }

        //工具方法
        private string ToPaeameter(object source, bool IsStrUpper = false)
        {
            var buff = new StringBuilder(string.Empty);
            if (source == null)
                throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source).Sort())
            {
                object value = property.GetValue(source);
                if (value != null)
                {
                    if (IsStrUpper)
                    {
                        buff.Append(WebUtility.UrlEncode(property.Name) + "=" + WebUtility.UrlEncode(value + "") + "&");
                    }
                    else
                    {
                        buff.Append(WebUtility.UrlEncode(GetlowerStr(property.Name)) + "=" + WebUtility.UrlEncode(value + "") + "&");
                    }
                }
            }
            return buff.ToString().Trim('&');
        }

        private static string GetlowerStr(object proname)
        {
            if (proname.ToString().Length >= 1)
            {
                return string.Format("{0}{1}", proname.ToString().Substring(0, 1).ToLower(), proname.ToString().Substring(1));
            }
            return string.Empty;
        }

        private string UrlEncode(string str)
        {
            string utf8Encoded = System.Web.HttpUtility.UrlEncode(str, Encoding.ASCII);
            return utf8Encoded.ToUpper();
        }

        private string GetRandomStr(int len = 12)
        {
            //首字母要为字母
            var guid = Math.Abs(Guid.NewGuid().GetHashCode()).ToString();
            var characters = "0123456789abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ";
            var Charsarr = new char[len];
            var random = new Random();
            for (int i = 0; i < Charsarr.Length; i++)
            {
                Charsarr[i] = characters[random.Next(characters.Length)];
            }
            var resultString = new String(Charsarr);
            return resultString;
        }
    }
}
using Neptune.NsPay.HttpExtensions.Bank.Helpers;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.RedisExtensions;
using Newtonsoft.Json;
using RestSharp;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Neptune.NsPay.AccountChecker.BankChecker
{
    public interface IBankChecker
    {
        Task<GetResult> GetHolderName(string accountNumber, string binNumber, bool isInternal);
    }


    public enum enAPIResponseError
    {
        AccountInvalid = 1,
        BankNotSupported = 2,
        SessionExpired = 3,
        SessionNotFound = 4,
        ApiCoolDown = 5,
        ApiTimeOut = 6,
        None = 0
    }

    public class GetResult
    {
        public string HolderName { get; set; }

        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public string SessionId { get; set; }

        public bool isSessionValid { get; set; }

        public enAPIResponseError ApiError { get; set; }
    }

    public class TCBChecker : IBankChecker
    {

        const string endpoint = "https://onlinebanking.techcombank.com.vn/api/payment-dis/client-api/v2/transfers/inquiry-accounts";
        private string bankAccount = string.Empty;
        private readonly IRedisService _redisService;
        private readonly LogHelper _LogHelper;

        public TCBChecker(string bankCardNumber, IRedisService redisService, LogHelper logHelper)
        {
            bankAccount = bankCardNumber;
            _redisService = redisService;
            _LogHelper = logHelper;
        }

        private async Task<TechcomBankLoginToken> GetLoginSession(string accountNumber)
        {
            return _redisService.GetRedisValue<TechcomBankLoginToken>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.TechcomBank, accountNumber));
        }

        public async Task<GetResult> GetHolderName(string accountNumber, string binNumber, bool isInternal)
        {
            var session = await GetLoginSession(bankAccount);
            GetResult returnResult = new GetResult() { };
            bool isSessionValid = false;
            StringBuilder logContent = new StringBuilder(" (" + bankAccount + " | TCB) ## (" + accountNumber + "| " + binNumber + ") ");

            if (session != null)
            {
                returnResult.SessionId = session.session_state; //unique identifier
                string transferType = isInternal ? "TCB_INTERNAL" : "TCB_EXTERNAL_NAPAS";
                var requestUrl = string.Format(@endpoint + "?accountNumber={0}&transferType={1}&bankCode={2}", accountNumber, transferType, binNumber);
                Uri targetUri = new Uri(endpoint);
                Stopwatch apiCounter = new Stopwatch();

                try
                {

                    apiCounter.Start();
                    using (var handler = new HttpClientHandler())
                    {
                        // Create a CookieContainer to store cookies
                        handler.CookieContainer = new CookieContainer();
                        // Initialize HttpClient with the handler
                        using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) })
                        {
                            // Set custom headers
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + session.access_token);
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            handler.CookieContainer.Add(targetUri, new Cookie("XSRF-TOKEN", session.session_state));
                            handler.CookieContainer.Add(targetUri, new Cookie("TS017c029b", "01e46b1260c8d62b72ff00f5187f9bf93656b93e9117c256b8ac0bd3583f20535a14545ac386c236f47d6c369659bd3815bcefc1853a336692819f53776b42cdcb29bf3cc9"));

                            // Send a GET request
                            HttpResponseMessage response = await client.GetAsync(new Uri(requestUrl));
                            returnResult.IsSuccess = response.IsSuccessStatusCode;
                            logContent.Append("Response Success - " + response.IsSuccessStatusCode + " Status Code - " + response.StatusCode);
                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                logContent.Append(" Response Content -  " + responseContent);
                                var responseObj = System.Text.Json.JsonSerializer.Deserialize<ResponseResult>(responseContent);

                                if (!string.IsNullOrEmpty(responseObj.customerName))
                                {
                                    returnResult.HolderName = responseObj.customerName;
                                    isSessionValid = true;
                                }


                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                returnResult.ApiError = enAPIResponseError.AccountInvalid;
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                returnResult.ApiError = enAPIResponseError.SessionExpired;
                            }
                            else
                            {
                                isSessionValid = true;
                            }
                        }
                    }
                }
                catch (TaskCanceledException ex2)
                {
                    // Catch the timeout exception
                    logContent.Append("Response TimeOut");
                    returnResult.ApiError = enAPIResponseError.ApiTimeOut;
                    _LogHelper.LogError(" TCBChecker ", ex2);
                }
                catch (Exception ex)
                {
                    _LogHelper.LogError(" TCBChecker ", ex);
                }
                finally
                {
                    apiCounter.Stop();
                    logContent.Append(" api time taken " + apiCounter.ElapsedMilliseconds + " ms");
                }
            }
            else
            {
                returnResult.Message = "Session Not Found";
                returnResult.ApiError = enAPIResponseError.SessionNotFound;
            }

            returnResult.isSessionValid = isSessionValid;
            _LogHelper.LogWarning(logContent.ToString());
            return returnResult;

        }
        public class ResponseResult
        {
            public string accountIdentifier { get; set; }
            public string customerName { get; set; }
            public bool readyToPay { get; set; }
            public bool autoBillRegistered { get; set; }
        }


    }


    public class MBChecker : IBankChecker
    {

        const string endpoint = "https://online.mbbank.com.vn/api/retail_web/transfer/inquiryAccountName";
        private string bankAccount = string.Empty;
        private string loginId = string.Empty;
        private readonly IRedisService _redisService;
        private readonly LogHelper _LogHelper;
        private string proxyURL = string.Empty;
        private int proxyPort = 0;
        private string proxyUser = string.Empty;
        private string proxyPassword = string.Empty;

        public MBChecker(string bankCardNumber, string accountID, IRedisService redisService, LogHelper logHelper, IConfiguration configuration)
        {
            bankAccount = bankCardNumber;
            _redisService = redisService;
            loginId = accountID;
            _LogHelper = logHelper;


            if (!string.IsNullOrEmpty(configuration["Proxy:Url"]))
            {
                proxyURL = configuration["Proxy:Url"].ToString();
            }


            if (!string.IsNullOrEmpty(configuration["Proxy:Port"]) && int.TryParse(configuration["Proxy:Port"].ToString(), out var _value))
            {
                proxyPort = _value;
            }

            if (!string.IsNullOrEmpty(configuration["Proxy:UserName"]))
            {
                proxyUser = configuration["Proxy:UserName"].ToString();
            }

            if (!string.IsNullOrEmpty(configuration["Proxy:Password"]))
            {
                proxyPassword = configuration["Proxy:Password"].ToString();
            }

        }

        private async Task<MbBankLoginModel> GetLoginSession(string accountNumber)
        {
            return _redisService.GetRedisValue<MbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.MBBank, accountNumber));
        }

        private string randomNumber()
        {

            Random rand = new Random();
            // Generate a random 6-digit number
            int randomNumber = rand.Next(10000, 99999); // The range is 100000 to 999999

            return randomNumber.ToString();
        }
        public async Task<GetResult> GetHolderName(string accountNumber, string binNumber, bool isInternal)
        {
            var session = await GetLoginSession(bankAccount);
            GetResult returnResult = new GetResult();
            bool isSessionValid = false;
            StringBuilder logContent = new StringBuilder(" (" + bankAccount + " | MB) ## (" + accountNumber + "| " + binNumber + ") ");
            if (session != null)
            {
                returnResult.SessionId = session.SessionId;
                var deviceId = session.SessionId.Split('&')[1];
                var sessionId = session.SessionId.Split('&')[0];
                var bodyContent = new Dictionary<string, object>();
                var refno = loginId + "-" + CommonHelper.GetDataString();
                bodyContent.Add("creditAccount", accountNumber);
                bodyContent.Add("creditAccountType", "ACCOUNT");
                bodyContent.Add("bankCode", binNumber);
                bodyContent.Add("debitAccount", bankAccount);
                bodyContent.Add("type", isInternal ? "INHOUSE" : "FAST");
                bodyContent.Add("remark", "");
                bodyContent.Add("sessionId", sessionId);
                bodyContent.Add("refNo", refno);
                bodyContent.Add("deviceIdCommon", deviceId);
                Stopwatch apiCounter = new Stopwatch();
                try
                {
                    var targetUri = new Uri(endpoint);
                    apiCounter.Start();
                    WebProxy webProxy = null;

                    if (!string.IsNullOrEmpty(proxyURL))
                    {
                        webProxy = new WebProxy(proxyURL, proxyPort);
                        webProxy.BypassProxyOnLocal = true;
                        webProxy.Credentials = new NetworkCredential(proxyUser, proxyPassword);
                    }


                    using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) })
                    {
                        var restClient = new RestClient(new RestClientOptions(endpoint)
                        {
                            Proxy = webProxy
                        });

                        string json = System.Text.Json.JsonSerializer.Serialize(bodyContent);


                        var request = new RestRequest()
                                .AddHeader("Authorization", "Basic RU1CUkVUQUlMV0VCOlNEMjM0ZGZnMzQlI0BGR0AzNHNmc2RmNDU4NDNm")
                                .AddHeader("Deviceid", deviceId)
                                .AddHeader("Refno", refno)
                                .AddParameter("application/json", json, ParameterType.RequestBody);
                        request.Timeout = 10000;
                        apiCounter.Start();
                        var response = await restClient.ExecutePostAsync(request);


                        returnResult.IsSuccess = false;

                        logContent.Append("Response Success -" + response.IsSuccessStatusCode + " Status Code - " + response.StatusCode);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = response.Content;
                            var responseObj = System.Text.Json.JsonSerializer.Deserialize<ResponseResult>(responseContent);

                            logContent.Append(" Response Content -  " + responseContent);

                            if (!string.IsNullOrEmpty(responseObj.benName))
                            {
                                returnResult.HolderName = responseObj.benName;
                                returnResult.IsSuccess = true;
                                isSessionValid = true;
                            }
                            else if (responseObj.result != null)
                            {
                                returnResult.Message = responseObj.result.message ?? responseObj.result.responseCode;

                                if (responseObj.result.responseCode != null)
                                {
                                    switch (responseObj.result.responseCode)
                                    {
                                        case "GW200":
                                            {
                                                isSessionValid = false;
                                                returnResult.ApiError = enAPIResponseError.SessionExpired;
                                                break;

                                            }
                                        case "MC201":
                                            {
                                                returnResult.ApiError = enAPIResponseError.AccountInvalid;
                                                break;

                                            }
                                        case "GW485":
                                            {
                                                returnResult.ApiError = enAPIResponseError.ApiCoolDown;
                                                break;

                                            }
                                    }
                                }

                            }
                        }
                        else
                        {
                            isSessionValid = true;
                        }

                    }
                }
                catch (TaskCanceledException ex2)
                {
                    // Catch the timeout exception
                    logContent.Append("Response TimeOut ");
                    returnResult.ApiError = enAPIResponseError.ApiTimeOut;
                    _LogHelper.LogError(" MBChecker ", ex2);
                }
                catch (Exception ex)
                {
                    _LogHelper.LogError(" MBChecker ", ex);
                }
                finally
                {
                    apiCounter.Stop();
                    logContent.Append(" api time taken " + apiCounter.ElapsedMilliseconds + " ms");
                }
            }
            else
            {
                returnResult.Message = "Session Not Found";
                returnResult.ApiError = enAPIResponseError.SessionNotFound;
            }

            returnResult.isSessionValid = isSessionValid;
            _LogHelper.LogWarning(logContent.ToString());
            return returnResult;
        }

        public class Result
        {
            public string message { get; set; }
            public bool ok { get; set; }
            public string responseCode { get; set; }
        }

        public class ResponseResult
        {
            public string refNo { get; set; }
            public Result result { get; set; }
            public string benName { get; set; }
            public string category { get; set; }
        }
    }


    public class BIDVChecker : IBankChecker
    {

        const string endpoint = "https://smartbanking.bidv.com.vn/w2/process";
        private string bankAccount = string.Empty;
        private string loginId = string.Empty;
        private readonly IRedisService _redisService;
        private readonly LogHelper _LogHelper;
        private string JSPath = string.Empty;
        private string NodeJSPath = string.Empty;

        public BIDVChecker(string bankCardNumber, string accountID, IRedisService redisService, LogHelper logHelper, IConfiguration configuration, string appRootPath)
        {
            bankAccount = bankCardNumber;
            _redisService = redisService;
            loginId = accountID;
            _LogHelper = logHelper;


            if (!string.IsNullOrEmpty(configuration["JSSetting:JSPath"]))
            {
                JSPath = Path.Combine(appRootPath, configuration["JSSetting:JSPath"].ToString());
            }

            if (!string.IsNullOrEmpty(configuration["JSSetting:NodeJSPath"]))
            {
                NodeJSPath = configuration["JSSetting:NodeJSPath"].ToString();
            }

        }

        private async Task<BidvBankLoginResponse> GetLoginSession(string accountNumber)
        {
            return _redisService.GetRedisValue<BidvBankLoginResponse>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BidvBank, accountNumber));
        }

        private string? BidvGetIntBenEncryptedRequest(string accNo, string browserId, string clientId, string appVersionId)
        {
            var jsFile = Path.Combine(JSPath, "BIDV", "encryptRequest_getinternalbeneficiary.js");
            if (!File.Exists(jsFile))
                return null;

            var jsonData = new BidvGetIntBenModel()
            {
                browserId = browserId,
                clientId = clientId,
                appVersionId = appVersionId,
                encrypt = new BidvGetIntBenEncryptModel()
                {
                    accNo = accNo,
                },
            };
            return LaunchJavascript(jsFile, JsonConvert.SerializeObject(jsonData).Replace("\\", "\\\\").Replace("\"", "\\\""));
        }

        private string? BidvGetExtBenEncryptedRequest(string accNo, string bankCode247, string browserId, string clientId, string appVersionId)
        {
            var jsFile = Path.Combine(JSPath, "BIDV", "encryptRequest_getexternalbeneficiary.js");
            if (!File.Exists(jsFile))
                return null;

            var jsonData = new BidvGetExtBenModel()
            {
                browserId = browserId,
                clientId = clientId,
                appVersionId = appVersionId,
                encrypt = new BidvGetExtBenEncryptModel()
                {
                    accNo = accNo,
                    bankCode247 = bankCode247
                },
            };
            return LaunchJavascript(jsFile, JsonConvert.SerializeObject(jsonData).Replace("\\", "\\\\").Replace("\"", "\\\""));
        }

        private string? BidvDecryptedResponse(BidvDecryptResponseModel model)
        {
            var jsFile = Path.Combine(JSPath, "BIDV", "decryptResponse.js");
            if (!File.Exists(jsFile))
                return null;

            return LaunchJavascript(jsFile, JsonConvert.SerializeObject(model).Replace("\\", "\\\\").Replace("\"", "\\\""));
        }

        private string? LaunchJavascript(string scriptFile, params string[] scriptArgs)
        {
            var result = string.Empty;

            var nodeExe = Path.Combine(NodeJSPath, "node.exe");
            if (!File.Exists(nodeExe))
                return null;

            var command = new StringBuilder();
            command.AppendFormat($"\"{scriptFile}\" ");
            command.Append($"\"{scriptArgs[0]}\"");

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                FileName = nodeExe,
                Arguments = command.ToString(),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Debug.WriteLine($"Javascript output: {Regex.Replace(output, @"\r\n?|\n", " ")}");

            if (output.Contains("done!"))
            {
                var spliter = new string[] { "\r\n", "\n" };
                var outputs = output.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                result = outputs.Length > 1 ? outputs[0] : "";
            }
            else if (output.Contains("error!"))
            {
                result = null;
            }

            return result;
        }

        public async Task<GetResult> GetHolderName(string accountNumber, string binNumber, bool isInternal)
        {
            var session = await GetLoginSession(bankAccount);
            GetResult returnResult = new GetResult();
            bool isSessionValid = false;
            StringBuilder logContent = new StringBuilder(" (" + bankAccount + " | BIDV) ## (" + accountNumber + "| " + binNumber + ") ");
            if (session != null && session.paySessionModel != null)
            {
                returnResult.SessionId = session.paySessionModel.Token; // unique identifier
                Stopwatch apiCounter = new Stopwatch();
                try
                {
                    var fName = "BidvGetExtBeneficiary";

                    string encrypted = null;
                    if (isInternal)
                    {
                        encrypted = BidvGetIntBenEncryptedRequest(accountNumber, session.paySessionModel.BrowserId, session.paySessionModel.ClientId, session.paySessionModel.AppVersionId);
                    }
                    else
                    {
                        encrypted = BidvGetExtBenEncryptedRequest(accountNumber, binNumber, session.paySessionModel.BrowserId, session.paySessionModel.ClientId, session.paySessionModel.AppVersionId);
                    }

                    if (encrypted != null)
                    {
                        var model = JsonConvert.DeserializeObject<BidvEncryptedRequestModel>(encrypted);
                        apiCounter.Start();

                        using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) })
                        {
                            var restClient = new RestClient(new RestClientOptions(endpoint));

                            var request = new RestRequest()
                                    .AddHeader("Authorization", session.paySessionModel.Token)
                                    .AddHeader("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36")
                                    .AddHeader("X-Request-ID", model.x_request_id)
                                .AddParameter("application/json", model.body, ParameterType.RequestBody);

                            if (!string.IsNullOrWhiteSpace(session.paySessionModel.Cookie))
                                request.AddHeader("Cookie", session.paySessionModel.Cookie);

                            request.Timeout = 10000;
                            apiCounter.Start();
                            var response = await restClient.ExecutePostAsync(request);

                            returnResult.IsSuccess = false;

                            logContent.Append("Response Success -" + response.IsSuccessStatusCode + " Status Code - " + response.StatusCode);

                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = response.Content;

                                var decrypt = JsonConvert.DeserializeObject<BidvDecryptResponseModel>(responseContent);
                                var decrpyted = BidvDecryptedResponse(decrypt);
                                var responseObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(decrpyted);

                                // var responseObj = System.Text.Json.JsonSerializer.Deserialize<ResponseResult>(responseContent);

                                logContent.Append(" Response Content -  " + JsonConvert.SerializeObject(decrpyted));

                                var recipientHolderFields = isInternal ? "accName" : "beneName";


                                if (responseObj.ContainsKey(recipientHolderFields) && !string.IsNullOrEmpty(responseObj[recipientHolderFields].ToString()))
                                {
                                    returnResult.HolderName = responseObj[recipientHolderFields].ToString();
                                    returnResult.IsSuccess = true;
                                    isSessionValid = true;
                                }
                                //else if (responseObj.result != null)
                                //{
                                //    returnResult.Message = responseObj.result.message ?? responseObj.result.responseCode;

                                //    if (responseObj.result.responseCode != null)
                                //    {
                                //        switch (responseObj.result.responseCode)
                                //        {
                                //            case "GW200":
                                //                {
                                //                    isSessionValid = false;
                                //                    returnResult.ApiError = enAPIResponseError.SessionExpired;
                                //                    break;

                                //                }
                                //            case "MC201":
                                //                {
                                //                    returnResult.ApiError = enAPIResponseError.AccountInvalid;
                                //                    break;

                                //                }
                                //            case "GW485":
                                //                {
                                //                    returnResult.ApiError = enAPIResponseError.ApiCoolDown;
                                //                    break;

                                //                }
                                //        }
                                //    }

                                //}
                            }
                            else
                            {
                                isSessionValid = true;
                            }

                        }
                    }




                }
                catch (TaskCanceledException ex2)
                {
                    // Catch the timeout exception
                    logContent.Append("Response TimeOut ");
                    returnResult.ApiError = enAPIResponseError.ApiTimeOut;
                    _LogHelper.LogError(" BIDVChecker ", ex2);
                }
                catch (Exception ex)
                {
                    _LogHelper.LogError(" BIDVChecker ", ex);
                }
                finally
                {
                    apiCounter.Stop();
                    logContent.Append(" api time taken " + apiCounter.ElapsedMilliseconds + " ms");
                }
            }
            else
            {
                returnResult.Message = "Session Not Found";
                returnResult.ApiError = enAPIResponseError.SessionNotFound;
            }

            returnResult.isSessionValid = isSessionValid;
            _LogHelper.LogWarning(logContent.ToString());
            return returnResult;
        }

        public class Result
        {
            public string message { get; set; }
            public bool ok { get; set; }
            public string responseCode { get; set; }
        }

        public class ResponseResult
        {
            public string refNo { get; set; }
            public Result result { get; set; }
            public string benName { get; set; }
            public string category { get; set; }
        }
    }


    public class VTBChecker : IBankChecker
    {

        const string endpoint = "https://api-ipay.vietinbank.vn";
        private string bankAccount = string.Empty;
        private string loginId = string.Empty;
        private readonly IRedisService _redisService;
        private readonly LogHelper _LogHelper;
        private string JSPath = string.Empty;
        private string NodeJSPath = string.Empty;

        public VTBChecker(string bankCardNumber, string accountID, IRedisService redisService, LogHelper logHelper, IConfiguration configuration, string appRootPath)
        {
            bankAccount = bankCardNumber;
            _redisService = redisService;
            loginId = accountID;
            _LogHelper = logHelper;


            if (!string.IsNullOrEmpty(configuration["JSSetting:JSPath"]))
            {
                JSPath = Path.Combine(appRootPath, configuration["JSSetting:JSPath"].ToString());
            }

            if (!string.IsNullOrEmpty(configuration["JSSetting:NodeJSPath"]))
            {
                NodeJSPath = configuration["JSSetting:NodeJSPath"].ToString();
            }

        }

        private async Task<VtbBankLoginModel> GetLoginSession(string accountNumber)
        {
            return _redisService.GetRedisValue<VtbBankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.VietinBank, accountNumber));
        }


        private string? LaunchJavascript(string scriptFile, params string[] scriptArgs)
        {
            var result = string.Empty;

            var nodeExe = Path.Combine(NodeJSPath, "node.exe");
            if (!File.Exists(nodeExe))
                return null;

            var command = new StringBuilder();
            command.AppendFormat($"\"{scriptFile}\" ");
            command.Append($"\"{scriptArgs[0]}\"");

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                FileName = nodeExe,
                Arguments = command.ToString(),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Debug.WriteLine($"Javascript output: {Regex.Replace(output, @"\r\n?|\n", " ")}");

            if (output.Contains("done!"))
            {
                var spliter = new string[] { "\r\n", "\n" };
                var outputs = output.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                result = outputs.Length > 1 ? outputs[0] : "";
            }
            else if (output.Contains("error!"))
            {
                result = null;
            }

            return result;
        }

        private string? VtbGetExtBenEncryptedRequest(string fromAccount, string beneficiarType, string beneficiaryAccount, string beneficiarBin, string sessionId)
        {
            var jsFile = Path.Combine(JSPath, "Vtb", "encryptRequest_napastransfer.js");
            if (!File.Exists(jsFile))
                return null;

            var jsonData = new VtbGetExtBenModel()
            {
                sessionId = sessionId,
                encrypt = new VtbGetExtBenEncryptModel()
                {
                    fromAccount = fromAccount,
                    beneficiaryType = beneficiarType,
                    beneficiaryAccount = beneficiaryAccount,
                    beneficiaryBin = beneficiarBin,
                },
            };
            return LaunchJavascript(jsFile, JsonConvert.SerializeObject(jsonData).Replace("\\", "\\\\").Replace("\"", "\\\""));
        }


        private string? VtbGetIntBenEncryptedRequest(string accountNumber, string sessionId)
        {
            var jsFile = Path.Combine(JSPath, "Vtb", "encryptRequest_internaltransfer.js");
            if (!File.Exists(jsFile))
                return null;

            var jsonData = new VtbGetIntBenModel()
            {
                sessionId = sessionId,
                encrypt = new VtbGetIntBenEncryptModel()
                {
                    accountNumber = accountNumber,
                },
            };
            return LaunchJavascript(jsFile, JsonConvert.SerializeObject(jsonData).Replace("\\", "\\\\").Replace("\"", "\\\""));
        }


        public async Task<GetResult> GetHolderName(string accountNumber, string binNumber, bool isInternal)
        {
            var session = await GetLoginSession(bankAccount);
            GetResult returnResult = new GetResult();
            bool isSessionValid = false;
            StringBuilder logContent = new StringBuilder(" (" + bankAccount + " | VTB) ## (" + accountNumber + "| " + binNumber + ") ");
            if (session != null && !string.IsNullOrEmpty(session.sessionid))
            {

                Stopwatch apiCounter = new Stopwatch();
                returnResult.SessionId = session.sessionid; // unique identifier
                try
                {
                    var finalEndpoint = isInternal ? (endpoint + "/ipay/wa/makeInternalTransfer") : (endpoint + "/ipay/wa/napasTransfer");

                    string encrypted = null;
                    if (isInternal)
                    {
                        encrypted = VtbGetIntBenEncryptedRequest(accountNumber, session.sessionid);
                    }
                    else
                    {
                        encrypted = VtbGetExtBenEncryptedRequest(bankAccount, "account", accountNumber, binNumber, session.sessionid);
                    }

                    if (encrypted != null)
                    {
                        var model = new VtbCommonEncryptedRequestModel() { encrypted = encrypted };
                        apiCounter.Start();

                        using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) })
                        {
                            var restClient = new RestClient(new RestClientOptions(finalEndpoint));

                            var request = new RestRequest()
                                    .AddHeader("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36")
                                .AddParameter("application/json", model, ParameterType.RequestBody);

                            request.Timeout = 10000;
                            apiCounter.Start();
                            var response = await restClient.ExecutePostAsync(request);
                            returnResult.IsSuccess = false;
                            string responseContent = response.Content;
                            logContent.Append("Response Success -" + response.IsSuccessStatusCode + " Status Code - " + response.StatusCode + " Content " + responseContent);


                            if (response.IsSuccessStatusCode)
                            {

                                var responseObj = JsonConvert.DeserializeObject<VtbBankResponseModel>(responseContent);

                                if (!responseObj.error)
                                {
                                    if (isInternal && !string.IsNullOrEmpty(responseObj.toAccountName))
                                    {
                                        returnResult.HolderName = responseObj.toAccountName;
                                    }
                                    else if (!string.IsNullOrEmpty(responseObj.beneficiaryName))
                                    {
                                        returnResult.HolderName = responseObj.beneficiaryName;
                                    }
                                    returnResult.IsSuccess = true;
                                    isSessionValid = true;
                                }

                            }
                            else
                            {
                                isSessionValid = true;
                                var responseObj = JsonConvert.DeserializeObject<VtbBankResponseModel>(responseContent);

                                if (responseObj != null && responseObj.error)
                                {
                                    if (!string.IsNullOrEmpty(responseObj.errorCode) && responseObj.errorCode == "96") // invalid Session
                                    {
                                        isSessionValid = false;
                                        returnResult.ApiError = enAPIResponseError.SessionExpired;
                                    }

                                }
                            }

                        }
                    }

                }
                catch (TaskCanceledException ex2)
                {
                    // Catch the timeout exception
                    logContent.Append("Response TimeOut ");
                    returnResult.ApiError = enAPIResponseError.ApiTimeOut;
                    _LogHelper.LogError(" VTBChecker ", ex2);
                }
                catch (Exception ex)
                {
                    _LogHelper.LogError(" VTBChecker ", ex);
                }
                finally
                {
                    apiCounter.Stop();
                    logContent.Append(" api time taken " + apiCounter.ElapsedMilliseconds + " ms");
                }
            }
            else
            {
                returnResult.Message = "Session Not Found";
                returnResult.ApiError = enAPIResponseError.SessionNotFound;
            }

            returnResult.isSessionValid = isSessionValid;
            _LogHelper.LogWarning(logContent.ToString());
            return returnResult;
        }

        public class Result
        {
            public string message { get; set; }
            public bool ok { get; set; }
            public string responseCode { get; set; }
        }

        public class ResponseResult
        {
            public string refNo { get; set; }
            public Result result { get; set; }
            public string benName { get; set; }
            public string category { get; set; }
        }
    }


    public class SEAChecker : IBankChecker
    {

        const string endpoint = "https://ebankms2.seanet.vn";
        private string bankAccount = string.Empty;
        private readonly IRedisService _redisService;
        private readonly LogHelper _LogHelper;

        public SEAChecker(string bankCardNumber, IRedisService redisService, LogHelper logHelper)
        {
            bankAccount = bankCardNumber;
            _redisService = redisService;
            _LogHelper = logHelper;
        }

        private async Task<BankLoginModel> GetLoginSession(string accountNumber)
        {
            return _redisService.GetRedisValue<BankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.SeaBank, accountNumber));
        }

        public async Task<GetResult> GetHolderName(string accountNumber, string binNumber, bool isInternal)
        {
            var session = await GetLoginSession(bankAccount);
            GetResult returnResult = new GetResult() { };
            bool isSessionValid = false;
            StringBuilder logContent = new StringBuilder(" (" + bankAccount + " | SEA) ## (" + accountNumber + "| " + binNumber + ") ");

            if (session != null)
            {
                returnResult.SessionId = session.Token; //unique identifier
                string relativePath = isInternal ? $"/p0405/api/swib-enquiry/check-customer-info/{accountNumber}" : "/p0405/api/common/enq-check-acc";
                var requestUrl = string.Format(@endpoint + relativePath);
                Uri targetUri = new Uri(requestUrl);
                Stopwatch apiCounter = new Stopwatch();

                try
                {

                    apiCounter.Start();
                    using (var handler = new HttpClientHandler())
                    {
                        // Create a CookieContainer to store cookies
                        handler.CookieContainer = new CookieContainer();
                        // Initialize HttpClient with the handler
                        using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) })
                        {
                            // Set custom headers
                            client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
                            client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + session.Token.Replace("Bearer ", String.Empty)));
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            // Send a GET request
                            HttpResponseMessage response;

                            if (isInternal)
                            {
                                response = await client.GetAsync(targetUri);
                            }
                            else
                            {
                                string jsonData = System.Text.Json.JsonSerializer.Serialize(new SeaGetExtBenModel { bankID = binNumber, benAccount = accountNumber, senderAccount = bankAccount });
                                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                                response = await client.PostAsync(targetUri, content);

                            }

                            returnResult.IsSuccess = response.IsSuccessStatusCode;
                            logContent.Append("Response Success - " + response.IsSuccessStatusCode + " Status Code - " + response.StatusCode);

                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                logContent.Append(" Response Content -  " + responseContent);

                                if (isInternal)
                                {
                                    var responseObj = System.Text.Json.JsonSerializer.Deserialize<SeaIntResponseModel>(responseContent);

                                    if (responseObj.code == "00" && responseObj.data != null && responseObj.data.account != null && responseObj.data.account.Count > 0
                                        && !string.IsNullOrEmpty(responseObj.data.account.First().shortName))
                                    {
                                        returnResult.HolderName = responseObj.data.account.First().shortName;
                                        isSessionValid = true;
                                    }
                                }
                                else
                                {
                                    var responseObj = System.Text.Json.JsonSerializer.Deserialize<SeaResponseModel>(responseContent);

                                    if (responseObj.code == "00" && responseObj.data != null && responseObj.data.accountInfo != null && !string.IsNullOrEmpty(responseObj.data.accountInfo.accountName))
                                    {
                                        returnResult.HolderName = responseObj.data.accountInfo.accountName;
                                        isSessionValid = true;
                                    }
                                }
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                returnResult.ApiError = enAPIResponseError.AccountInvalid;
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                returnResult.ApiError = enAPIResponseError.SessionExpired;
                            }
                            else
                            {
                                isSessionValid = true;
                            }
                        }
                    }
                }
                catch (TaskCanceledException ex2)
                {
                    // Catch the timeout exception
                    logContent.Append("Response TimeOut");
                    returnResult.ApiError = enAPIResponseError.ApiTimeOut;
                    _LogHelper.LogError(" SEAChecker ", ex2);
                }
                catch (Exception ex)
                {
                    _LogHelper.LogError(" SEAChecker ", ex);
                }
                finally
                {
                    apiCounter.Stop();
                    logContent.Append(" api time taken " + apiCounter.ElapsedMilliseconds + " ms");
                }
            }
            else
            {
                returnResult.Message = "Session Not Found";
                returnResult.ApiError = enAPIResponseError.SessionNotFound;
            }

            returnResult.isSessionValid = isSessionValid;
            _LogHelper.LogWarning(logContent.ToString());
            return returnResult;

        }
        public class ResponseResult
        {
            public string accountIdentifier { get; set; }
            public string customerName { get; set; }
            public bool readyToPay { get; set; }
            public bool autoBillRegistered { get; set; }
        }


    }



    public class MSBChecker : IBankChecker
    {

        const string endpoint = "https://ebank.msb.com.vn";
        private string bankAccount = string.Empty;
        private readonly IRedisService _redisService;
        private readonly LogHelper _LogHelper;

        public MSBChecker(string bankCardNumber, IRedisService redisService, LogHelper logHelper)
        {
            bankAccount = bankCardNumber;
            _redisService = redisService;
            _LogHelper = logHelper;
        }

        private async Task<BankLoginModel> GetLoginSession(string accountNumber)
        {
            return _redisService.GetRedisValue<BankLoginModel>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.MsbBank, accountNumber));
        }

        public async Task<GetResult> GetHolderName(string accountNumber, string binNumber, bool isInternal)
        {
            var session = await GetLoginSession(bankAccount);
            GetResult returnResult = new GetResult() { };
            bool isSessionValid = false;
            StringBuilder logContent = new StringBuilder(" (" + bankAccount + " | MSB) ## (" + accountNumber + "| " + binNumber + ") ");

            if (session != null)
            {
                returnResult.SessionId = session.Token; //unique identifier
                string relativePath = isInternal ? "/IBSRetail/account/getBenefitName.do" : "/IBSRetail/account/getBenefitSML.do";
                var requestUrl = string.Format(@endpoint + relativePath);
                Uri targetUri = new Uri(requestUrl);
                Stopwatch apiCounter = new Stopwatch();

                try
                {

                    apiCounter.Start();
                    using (var handler = new HttpClientHandler())
                    {
                        // Create a CookieContainer to store cookies
                        handler.CookieContainer = new CookieContainer();
                        // Initialize HttpClient with the handler
                        using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) })
                        {
                            // Set custom headers
                            client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
                            client.DefaultRequestHeaders.Add("Cookie", session.Cookies);
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var modelJsonContent = string.Empty;

                            if (isInternal)
                            {
                                var model = new MsbGetIntBenModel()
                                {
                                    toBenefitAcc = accountNumber,
                                    tokenNo = session.Token,
                                    lang = "en_US",
                                };

                                modelJsonContent = System.Text.Json.JsonSerializer.Serialize(model);
                            }
                            else
                            {
                                var model = new MsbGetExtBenModel()
                                {
                                    bankCode = binNumber,
                                    beneficiaryAccount = accountNumber,
                                    type = "acctType",
                                    fromAcct = bankAccount,
                                    tokenNo = session.Token,
                                    lang = "en_US",
                                };

                                modelJsonContent = System.Text.Json.JsonSerializer.Serialize(model);
                            }

                            var formData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(modelJsonContent);

                            // Create the content with media type application/x-www-form-urlencoded
                            var content = new FormUrlEncodedContent(formData);

                            // Send a POST request
                            HttpResponseMessage response = await client.PostAsync(targetUri, content);

                            returnResult.IsSuccess = response.IsSuccessStatusCode;
                            logContent.Append("Response Success - " + response.IsSuccessStatusCode + " Status Code - " + response.StatusCode);
                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                logContent.Append(" Response Content -  " + responseContent);

                                if (isInternal)
                                {
                                    var responseObj = System.Text.Json.JsonSerializer.Deserialize<MsIntBenResponseModel>(responseContent);

                                    if (responseObj.status == "200" && responseObj.data != null && !string.IsNullOrEmpty(responseObj.data.name))
                                    {
                                        returnResult.HolderName = responseObj.data.name;
                                        isSessionValid = true;
                                    }


                                }
                                else
                                {
                                    var responseObj = System.Text.Json.JsonSerializer.Deserialize<MsbExtBenResponseModel>(responseContent);

                                    if (responseObj.status == "200" && responseObj.data != null && !string.IsNullOrEmpty(responseObj.data.beneficiaryName))
                                    {
                                        returnResult.HolderName = responseObj.data.beneficiaryName;
                                        isSessionValid = true;
                                    }
                                }



                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                returnResult.ApiError = enAPIResponseError.AccountInvalid;
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                returnResult.ApiError = enAPIResponseError.SessionExpired;
                            }
                            else
                            {
                                isSessionValid = true;
                            }
                        }
                    }
                }
                catch (TaskCanceledException ex2)
                {
                    // Catch the timeout exception
                    logContent.Append("Response TimeOut");
                    returnResult.ApiError = enAPIResponseError.ApiTimeOut;
                    _LogHelper.LogError(" MSBChecker ", ex2);
                }
                catch (Exception ex)
                {
                    _LogHelper.LogError(" MSBChecker ", ex);
                }
                finally
                {
                    apiCounter.Stop();
                    logContent.Append(" api time taken " + apiCounter.ElapsedMilliseconds + " ms");
                }
            }
            else
            {
                returnResult.Message = "Session Not Found";
                returnResult.ApiError = enAPIResponseError.SessionNotFound;
            }

            returnResult.isSessionValid = isSessionValid;
            _LogHelper.LogWarning(logContent.ToString());
            return returnResult;

        }
        public class ResponseResult
        {
            public string accountIdentifier { get; set; }
            public string customerName { get; set; }
            public bool readyToPay { get; set; }
            public bool autoBillRegistered { get; set; }
        }


    }
}

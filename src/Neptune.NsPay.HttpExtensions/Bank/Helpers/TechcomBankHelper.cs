using Abp.Extensions;
using Abp.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using System.Net;
using System.Text;
using WebProxy = System.Net.WebProxy;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers
{
    //大部分需要手机登录，所有记录登录的信息，直接查询 
    public class TechcomBankHelper : ITechcomBankHelper
    {
        private readonly IRedisService _redisService;

        private readonly static string TokenUrl = "https://identity-tcb.techcombank.com.vn/auth/realms/backbase/protocol/openid-connect/token";
        private readonly static string TransactionHistoryUrl = "https://onlinebanking.techcombank.com.vn/api/transaction-manager/client-api/v2/transactions?from=0&size=20&orderBy=bookingDate&direction=DESC";

        private readonly static string socksUrl = AppSettings.Configuration["SocksUrl"];
        private readonly static int socksPort = AppSettings.Configuration["SocksPort"].ToInt();
        private readonly static string socksusername = AppSettings.Configuration["SocksUsername"];
        private readonly static string sockspassword = AppSettings.Configuration["SocksPassword"];

        public TechcomBankHelper(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<bool> Verify(string cardNo)
        {
            var cacheToken = GetToken(cardNo);
            if (cacheToken != null)
            {
                TimeSpan timeSpan = DateTime.Now - cacheToken.CreateTime;
                if (timeSpan.TotalMinutes > 2.5)
                {
                    var result = await RefreshTokenAsync(cardNo);
                    if (result != null)
                    {
                        return true;
                    }
                    else
                    {
                        cacheToken.ErrorCount = cacheToken.ErrorCount + 1;
                        cacheToken.CreateTime = DateTime.Now;
                        SetToken(cardNo, cacheToken);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }


        //4分钟刷新一次token
        public async Task<TechcomBankLoginToken> RefreshTokenAsync(string cardNo)
        {
            try
            {
                var cacheToken = GetToken(cardNo);
                if (cacheToken != null)
                {
                    var client = new RestClient(TokenUrl);
                    if (!socksUrl.IsNullOrEmpty())
                    {
                        WebProxy webProxy = new WebProxy(socksUrl, socksPort);
                        webProxy.BypassProxyOnLocal = true;
                        webProxy.Credentials = new NetworkCredential(socksusername, sockspassword);
                        client = new RestClient(new RestClientOptions(TokenUrl)
                        {
                            Proxy = webProxy
                        });
                    }
                    var request = new RestRequest()
                        .AddHeader("Cookie", cacheToken.cookie)
                        .AddHeader("Content-Type", "application/x-www-form-urlencoded")
                        .AddParameter("grant_type", "refresh_token")
                        .AddParameter("refresh_token", cacheToken.refresh_token)
                        .AddParameter("client_id", "tcb-web-client");
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var response = await client.ExecutePostAsync(request);
                    if (response == null) return null;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var detailResponse = response.Content.FromJsonString<TechcomBankLoginToken>();
                        if (detailResponse != null)
                        {
                            detailResponse.cookie = cacheToken.cookie;
                            detailResponse.CreateTime = DateTime.Now;
                            ReplaceToken(cardNo, detailResponse);
                            return detailResponse;
                        }
                        else
                        {
                            NlogLogger.Error("账户：" + cardNo + "刷新token返回数据：" + response.Content.ToJsonString());
                        }
                    }
                    else
                    {
                        NlogLogger.Error("账户：" + cardNo + "刷新token返回数据：" + response.ToJsonString());
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + cardNo + "刷新token异常：" + ex.ToString());
                return null;
            }
        }

        //获取订单详情
        public async Task<List<TechcomBankTransactionHistoryResponse>> GetTransactionHistory(string cardNo)
        {
            try
            {
                var cacheToken = GetToken(cardNo);
                if (cacheToken != null)
                {
                    WebProxy webProxy = new WebProxy(socksUrl, socksPort);
                    webProxy.BypassProxyOnLocal = true;
                    webProxy.Credentials = new NetworkCredential(socksusername, sockspassword);

                    //获取arrangementsid
                    //await TestIpAsync(0);
                    var arrangidUrl = "https://onlinebanking.techcombank.com.vn/api/arrangement-manager/client-api/v2/productsummary/context/arrangements?businessFunction=Product%20Summary&resourceName=Product%20Summary&privilege=view&productKindName=Current%20Account&size=1000000";
                    var arrclient = new RestClient(arrangidUrl);
                    if (!socksUrl.IsNullOrEmpty())
                    {
                        arrclient = new RestClient(new RestClientOptions(arrangidUrl)
                        {
                            Proxy = webProxy
                        });
                    }
                    var arrrequest = new RestRequest()
                        .AddHeader("Cookie", GetTransactionCookie(cacheToken.cookie, cacheToken.access_token, cacheToken.session_state));
                    var responseArr = await arrclient.ExecuteGetAsync(arrrequest);
                    if (responseArr.StatusCode != HttpStatusCode.OK)
                    {
                        cacheToken.ErrorCount = cacheToken.ErrorCount + 1;
                        cacheToken.CreateTime = DateTime.Now;
                        SetToken(cardNo, cacheToken);
                        return null;
                    }
                    if (responseArr.Content.IsNullOrEmpty())
                    {
                        cacheToken.ErrorCount = cacheToken.ErrorCount + 1;
                        cacheToken.CreateTime = DateTime.Now;
                        SetToken(cardNo, cacheToken);
                        NlogLogger.Error("账户：" + cardNo + ",detailResponseArr返回数据：" + responseArr.ToJsonString());
                        return null;
                    }
                    var detailResponseArr = responseArr.Content.FromJsonString<List<TechcomBankArrangementsResponse>>();
                    if (detailResponseArr == null) return null;
                    if (detailResponseArr.Count <= 0) return null;
                    var arrangementsId = detailResponseArr[0].id;

                    //请求arr
                    var postArrUrl = "https://onlinebanking.techcombank.com.vn/api/sync-dis/client-api/v1/transactions/refresh/arrangements";
                    var postdata = "{\"externalArrangementIds\":[\"" + detailResponseArr[0].BBAN + "\"]}";
                    var postarrclient = new RestClient(postArrUrl);
                    if (!socksUrl.IsNullOrEmpty())
                    {
                        postarrclient = new RestClient(new RestClientOptions(postArrUrl)
                        {
                            Proxy = webProxy
                        });
                    }
                    var postarrrequest = new RestRequest()
                        .AddHeader("Cookie", GetTransactionCookie(cacheToken.cookie, cacheToken.access_token, cacheToken.session_state))
                        .AddParameter("application/json", postdata, ParameterType.RequestBody);
                    var postarr = await postarrclient.PostAsync(postarrrequest);

                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    Thread.Sleep(1000 * rnd.Next(2));
                    //await TestIpAsync(1);

                    var techcomBanks = new List<TechcomBankTransactionHistoryResponse>();
                    for (var i = 0; i < 10; i++)
                    {
                        try
                        {
                            string startTime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                            string endTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                            var url = "https://onlinebanking.techcombank.com.vn/api/transaction-manager/client-api/v2/transactions?bookingDateGreaterThan=" + startTime + "&bookingDateLessThan=" + endTime + "&arrangementId=" + arrangementsId + "&from=" + i + "&size=20";
                            var client = new RestClient(url);
                            if (!socksUrl.IsNullOrEmpty())
                            {
                                client = new RestClient(new RestClientOptions(url)
                                {
                                    Proxy = webProxy
                                });
                            }
                            var request = new RestRequest()
                                .AddHeader("Cookie", GetTransactionCookie(cacheToken.cookie, cacheToken.access_token, cacheToken.session_state));
                            var response = await client.ExecuteGetAsync(request);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                if (!response.Content.IsNullOrEmpty())
                                {
                                    var detailResponse = response.Content.FromJsonString<List<TechcomBankTransactionHistoryResponse>>();
                                    if (detailResponse != null)
                                    {
                                        if (response != null)
                                        {
                                            techcomBanks.AddRange(detailResponse);
                                        }
                                    }
                                    else
                                    {
                                        cacheToken.ErrorCount = cacheToken.ErrorCount + 1;
                                        cacheToken.CreateTime = DateTime.Now;
                                        SetToken(cardNo, cacheToken);
                                        NlogLogger.Error("账户：" + cardNo + "返回数据1：" + response.Content.ToJsonString());
                                    }
                                }
                                else
                                {
                                    cacheToken.ErrorCount = cacheToken.ErrorCount + 1;
                                    cacheToken.CreateTime = DateTime.Now;
                                    SetToken(cardNo, cacheToken);
                                    NlogLogger.Error("账户：" + cardNo + "返回数据2：" + response.ToJsonString());
                                }
                            }
                            else
                            {
                                cacheToken.ErrorCount = cacheToken.ErrorCount + 1;
                                cacheToken.CreateTime = DateTime.Now;
                                SetToken(cardNo, cacheToken);
                                NlogLogger.Error("账户：" + cardNo + "返回数据3：" + response.ToJsonString());
                            }
                        }
                        catch (Exception ex)
                        {
                            cacheToken.ErrorCount = cacheToken.ErrorCount + 1;
                            cacheToken.CreateTime = DateTime.Now;
                            SetToken(cardNo, cacheToken);
                            NlogLogger.Error("账户：" + cardNo + "TechcomBank获取错误：" + ex);
                        }
                    }
                    return techcomBanks;
                }
                NlogLogger.Error("账户：" + cardNo + ",获取登录缓存错误");
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + cardNo + "TechcomBank获取错误：" + ex);
                return null;
            }
        }


        //企业银行获取记录
        public async Task<List<TechcomBankTransactionHistoryResponse>> GetBusinessTransactionHistory(string cardno, string account)
        {
            try
            {
                var cacheToken = GetBusinessToken(cardno);
                if (cacheToken != null)
                {
                    List<TechcomBankTransactionHistoryResponse> techcomBanks = new List<TechcomBankTransactionHistoryResponse>();
                    WebProxy webProxy = new WebProxy(socksUrl, socksPort);
                    webProxy.BypassProxyOnLocal = true;
                    webProxy.Credentials = new NetworkCredential(socksusername, sockspassword);
                    DateTime startTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
                    DateTime endTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
                    var checkTime = startTime.Hour;
                    if (checkTime == 0)
                    {
                        startTime = DateTime.Now.AddDays(-1);
                    }
                    endTime = endTime.AddDays(14);
                    var url = "https://business.techcombank.com.vn/api/transaction-manager/client-api/v2/transactions?bookingDateGreaterThan=" + startTime.ToString("yyyy-MM-dd") + "&bookingDateLessThan=" + endTime.ToString("yyyy-MM-dd") + "&arrangementsIds=" + cacheToken.arrangementsIds + "&from=0&size=200";
                    var client = new RestClient(url);
                    if (!socksUrl.IsNullOrEmpty())
                    {
                        client = new RestClient(new RestClientOptions(url)
                        {
                            Proxy = webProxy
                        });
                    }
                    var request = new RestRequest()
                        .AddHeader("Authorization", cacheToken.authorization)
                        .AddHeader("Cookie", cacheToken.cookie)
                        .AddHeader("Host", "business.techcombank.com.vn")
                        .AddHeader("Referer", "https://business.techcombank.com.vn/");
                    //await TestIpAsync(0);
                    var response = await client.ExecuteGetAsync(request);
                    //await TestIpAsync(1);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (!response.Content.IsNullOrEmpty())
                        {
                            var detailResponse = response.Content.FromJsonString<List<TechcomBankTransactionHistoryResponse>>();
                            if (detailResponse != null)
                            {
                                if (response != null)
                                {
                                    techcomBanks.AddRange(detailResponse);
                                    return techcomBanks;
                                }
                            }
                            else
                            {
                                NlogLogger.Error("企业账户：" + account + "返回数据1：" + response.Content.ToJsonString());
                                return null;
                            }
                        }
                        else
                        {
                            NlogLogger.Error("企业账户：" + account + "返回数据2：" + response.ToJsonString());
                            return null;
                        }
                    }
                    else
                    {
                        NlogLogger.Error("企业账户：" + account + "返回数据3：" + response.ToJsonString());
                    }
                    return null;
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("账户：" + account + "Business TechcomBank获取错误：" + ex);
                return null;
            }
        }


        public async Task<TechcomBankTransferResult> CreateTransfer(string account, TechcomBankTransferRequest transferRequest)
        {
            try
            {
                TechcomBankTransferResult transferResult = new TechcomBankTransferResult();
                var cacheToken = GetToken(account);
                if (cacheToken != null)
                {
                    //越南时间
                    var dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");

                    string filePath = "D:\\BankCode\\tcb.json";
                    var codes = File.ReadAllText(filePath);
                    var tcbBankCodes = JsonConvert.DeserializeObject<List<NaPasBankModel>>(codes);
                    var bank = tcbBankCodes.FirstOrDefault(r => r.shortName.ToLower() == transferRequest.BankName.ToLower());
                    if (bank != null)
                    {
                        WebProxy webProxy = new WebProxy(socksUrl, socksPort);
                        webProxy.BypassProxyOnLocal = true;
                        webProxy.Credentials = new NetworkCredential(socksusername, sockspassword);

                        if (string.IsNullOrEmpty(transferRequest.ArrangementId))
                        {
                            //获取arrangementsid
                            var arrangidUrl = "https://onlinebanking.techcombank.com.vn/api/arrangement-manager/client-api/v2/productsummary/context/arrangements?businessFunction=Product%20Summary&resourceName=Product%20Summary&privilege=view&productKindName=Current%20Account&size=1000000";
                            var arrclient = new RestClient(arrangidUrl);
                            if (!string.IsNullOrEmpty(socksUrl))
                            {
                                arrclient = new RestClient(new RestClientOptions(arrangidUrl)
                                {
                                    Proxy = webProxy
                                });
                            }
                            var arrrequest = new RestRequest()
                                .AddHeader("Cookie", GetTransactionCookie(cacheToken.cookie, cacheToken.access_token, cacheToken.session_state));
                            var responseArr = await arrclient.GetAsync<List<TechcomBankArrangementsResponse>>(arrrequest);
                            if (responseArr == null) return null;
                            if (responseArr.Count <= 0) return null;
                            var arrangementsId = responseArr[0].id;

                            transferResult.ArrangementId = arrangementsId;
                            transferRequest.ArrangementId = arrangementsId;
                        }

                        //检查转账银行卡是否正确
                        var checkCardUrl = "https://onlinebanking.techcombank.com.vn/api/payment-dis/client-api/v2/transfers/inquiry-accounts?accountNumber=" + transferRequest.BenAccountNo + "&transferType=TCB_EXTERNAL_NAPAS&bankCode=" + bank.napasId;
                        var checkCardClient = new RestClient(checkCardUrl);
                        if (!string.IsNullOrEmpty(socksUrl))
                        {
                            checkCardClient = new RestClient(new RestClientOptions(checkCardUrl)
                            {
                                Proxy = webProxy
                            });
                        }
                        var checkCardRequest = new RestRequest()
                            .AddHeader("Authorization", "Bearer " + cacheToken.access_token)
                            .AddHeader("Cookie", GetTransactionCookie(cacheToken.cookie, cacheToken.access_token, cacheToken.session_state));
                        var checkCardResponse = await checkCardClient.ExecuteGetAsync(checkCardRequest);
                        if (checkCardResponse.StatusCode == HttpStatusCode.OK)
                        {
                            var detailCardResponse = JsonConvert.DeserializeObject<TechcomBankCheckCardResponse>(checkCardResponse.Content);
                            if (detailCardResponse != null)
                            {
                                var tempName = RemoveDiacritics(transferRequest.BenAccountName).ToUpper();
                                var customerName = detailCardResponse.customerName;
                                if (tempName == detailCardResponse.customerName.ToUpper())
                                {
                                    var valiRequest = new
                                    {
                                        paymentType = "TCB_EXTERNAL_NAPAS",
                                        transferTransactionInformation = new
                                        {
                                            remittanceInformation = customerName,
                                            instructedAmount = new
                                            {
                                                amount = transferRequest.Money,
                                                currencyCode = "VND"
                                            },
                                            counterparty = new
                                            {
                                                name = customerName,
                                            },
                                            counterpartyBank = new
                                            {
                                                name = bank.napasId
                                            },
                                            counterpartyAccount = new
                                            {
                                                name = customerName,
                                                identification = new
                                                {
                                                    identification = transferRequest.BenAccountNo,//转账人卡号
                                                    schemeName = "BBAN"
                                                }
                                            }
                                        },
                                        additions = new
                                        {
                                            transactionRemarks = customerName,
                                            lang = "vi"
                                        },
                                        originatorAccount = new
                                        {
                                            name = transferRequest.Name,
                                            identification = new
                                            {
                                                identification = transferRequest.ArrangementId,//转账人卡号
                                                schemeName = "ID"
                                            }
                                        },
                                        requestedExecutionDate = dateTime.ToString("yyyy-MM-dd")
                                    };
                                    var validatePostData = JsonConvert.SerializeObject(valiRequest);
                                    var validateUrl = "https://onlinebanking.techcombank.com.vn/api/payment-order-service/client-api/v2/payment-orders/validate";
                                    var validateClient = new RestClient(validateUrl);
                                    if (!string.IsNullOrEmpty(socksUrl))
                                    {
                                        validateClient = new RestClient(new RestClientOptions(validateUrl)
                                        {
                                            Proxy = webProxy
                                        });
                                    }
                                    var validateRequest = new RestRequest()
                                        .AddHeader("Authorization", "Bearer " + cacheToken.access_token)
                                        .AddHeader("Cookie", GetTransactionCookie(cacheToken.cookie, cacheToken.access_token, cacheToken.session_state))
                                        .AddParameter("application/json", validatePostData, ParameterType.RequestBody);
                                    var validateResponse = await validateClient.ExecutePostAsync(validateRequest);
                                    if (validateResponse.StatusCode == HttpStatusCode.OK)
                                    {
                                        if (!validateResponse.Content.Contains("Unauthorized"))
                                        {
                                            var request = new
                                            {
                                                originatorAccount = new
                                                {
                                                    identification = new
                                                    {
                                                        identification = transferRequest.ArrangementId,
                                                        schemeName = "ID"
                                                    },
                                                    name = transferRequest.Name
                                                },
                                                requestedExecutionDate = dateTime.ToString("yyyy-MM-dd"),
                                                paymentType = "TCB_EXTERNAL_NAPAS",
                                                transferTransactionInformation = new
                                                {
                                                    instructedAmount = new
                                                    {
                                                        amount = transferRequest.Money,//不包含小数点
                                                        currencyCode = "VND"
                                                    },
                                                    counterparty = new
                                                    {
                                                        name = customerName,//转账人姓名
                                                    },
                                                    counterpartyAccount = new
                                                    {
                                                        identification = new
                                                        {
                                                            identification = transferRequest.BenAccountNo,//转账人卡号
                                                            schemeName = "BBAN"
                                                        }
                                                    },
                                                    counterpartyBank = new
                                                    {
                                                        name = bank.napasId//银行编号
                                                    },
                                                    remittanceInformation = customerName + " " + transferRequest.OrderId,//转账备注
                                                    messageToBank = customerName + " " + transferRequest.OrderId,//转账备注
                                                    additions = new
                                                    {
                                                        counterpartyBankName = bank.displayNameVi,//银行详细名称
                                                        counterpartyBankShortName = bank.shortName//银行编号
                                                    }
                                                },
                                                additions = new
                                                {
                                                    transactionRemarks = customerName + " " + transferRequest.OrderId,//转账备注
                                                    lang = "vi",
                                                    channel = "7399"
                                                }
                                            };
                                            //出款地址
                                            var postdata = JsonConvert.SerializeObject(request);
                                            var paymentorderurl = "https://onlinebanking.techcombank.com.vn/api/payment-order-service/client-api/v2/payment-orders";
                                            var restClient = new RestClient(paymentorderurl);
                                            if (!string.IsNullOrEmpty(socksUrl))
                                            {
                                                restClient = new RestClient(new RestClientOptions(paymentorderurl)
                                                {
                                                    Proxy = webProxy
                                                });
                                            }
                                            var restRequest = new RestRequest()
                                                .AddHeader("Authorization", "Bearer " + cacheToken.access_token)
                                                .AddHeader("Cookie", GetTransactionCookie(cacheToken.cookie, cacheToken.access_token, cacheToken.session_state))
                                                .AddParameter("application/json", postdata, ParameterType.RequestBody);
                                            var response = await restClient.ExecutePostAsync(restRequest);
                                            if (response.StatusCode == HttpStatusCode.Unauthorized)
                                            {
                                                if (!response.Content.Contains("Unauthorized"))
                                                {
                                                    var detailResponse = JsonConvert.DeserializeObject<TechcomBankPaymentOrderResponse>(response.Content);
                                                    if (detailResponse != null)
                                                    {
                                                        var scope = detailResponse.challenges.FirstOrDefault()?.scope;
                                                        scope = scope.Replace(":", "%3A");
                                                        //var scope = "confirmation%3A" + detailResponse.FirstOrDefault().confirmationId;
                                                        //var authUrl = "https://identity-tcb.techcombank.com.vn/auth/realms/backbase/protocol/openid-connect/auth?response_type=code&client_id=tcb-web-client&state=bGQ2ckJSeGhNczY0RlhnckpFUW9mVX5WdXdsNHFJZVBmV2hIM1RUZnhPd3lM&redirect_uri=https%3A%2F%2Fonlinebanking.techcombank.com.vn%2Ftransfers-payments%2Fpay-someone%3FtransferType%3Dother&scope=openid+" + scope + "&code_challenge=E3aVSrem3Fj_f_v0nu5ulqieK2_DoB72_gzEj3fy3bY&code_challenge_method=S256&nonce=bGQ2ckJSeGhNczY0RlhnckpFUW9mVX5WdXdsNHFJZVBmV2hIM1RUZnhPd3lM&acr_values=txn-signature-silver+txn-signature-gold&response_mode=fragment&ui_locales=vi";
                                                        var authUrl = "https://identity-tcb.techcombank.com.vn/auth/realms/backbase/protocol/openid-connect/auth?response_type=code&client_id=tcb-web-client&state=aUJLUWpBfndNdUYuWkdvVFYweFNrZHZjUzVBdzd6eWREa2NxYllFS0RBNkJx&redirect_uri=https%3A%2F%2Fonlinebanking.techcombank.com.vn%2Ftransfers-payments%2Fpay-someone%3FtransferType%3Dother&scope=openid+" + scope + "&code_challenge=2tINJgrB0c1jDZZbjZrHiqTDYJAIQ6okFykHVTFsGZs&code_challenge_method=S256&nonce=aUJLUWpBfndNdUYuWkdvVFYweFNrZHZjUzVBdzd6eWREa2NxYllFS0RBNkJx&acr_values=txn-face-signature-silver+txn-face-signature-gold&response_mode=fragment&ui_locales=vi";

                                                        var authClient = new RestClient(authUrl);
                                                        if (!string.IsNullOrEmpty(socksUrl))
                                                        {
                                                            authClient = new RestClient(new RestClientOptions(authUrl)
                                                            {
                                                                Proxy = webProxy
                                                            });
                                                        }
                                                        var authRequest = new RestRequest()
                                                            .AddHeader("Authorization", "Bearer " + cacheToken.access_token)
                                                            .AddHeader("Cookie", cacheToken.cookie);
                                                        var responseAuth = await authClient.ExecuteGetAsync(authRequest);
                                                        if (responseAuth != null)
                                                        {
                                                            //手机确定订单
                                                            transferResult.PayMentsOrderId = detailResponse.data.paymentorderid;
                                                            return transferResult;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                }
                            }
                            //下单失败
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        //银行卡编码错误
                    }
                    return transferResult;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetTransactionCookie(string cacheCookie, string accessToken, string sessionstate)
        {
            if (cacheCookie.IsNullOrEmpty())
                return "";
            var arr = cacheCookie.Split(';');
            var gastr = "";
            var dtcookie = "";
            var _ga_T510PJVBBQ = "";
            foreach (var item in arr)
            {
                if (item.Contains("_ga="))
                {
                    gastr = item;
                }
                if (item.Contains("dtCookie="))
                {
                    dtcookie = item;
                }
                if (item.Contains("_ga_"))
                {
                    _ga_T510PJVBBQ = item;
                }
            }

            var cookie = gastr + "; arp_scroll_position=203.1999969482422; " + _ga_T510PJVBBQ + "; Authorization=" + accessToken + "; XSRF-TOKEN=" + sessionstate + "; TCB0140a57a=01961804621abb9302cd4cf465c835aea1422491d0aeebb7525ba3ffac8c1e71ce0c423c5403f1a35eb4125870e0c7a430c27e3f8a7c3667d35ce868c1732f1359705cc6d7824a181a339aaab315a4085ae13788eb; " + dtcookie;

            return cookie;
        }

        public TechcomBankLoginToken GetToken(string account)
        {
            return _redisService.GetRedisValue<TechcomBankLoginToken>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.TechcomBank, account));
        }

        public void SetToken(string account, TechcomBankLoginToken token)
        {
            _redisService.AddRedisValue<TechcomBankLoginToken>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.TechcomBank, account), token);
        }

        public BusinessTechcomBankLoginToken GetBusinessToken(string account)
        {
            return _redisService.GetRedisValue<BusinessTechcomBankLoginToken>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BusinessTcbBank, account));
        }

        public void SetBusinessToken(string account, BusinessTechcomBankLoginToken token)
        {
            _redisService.AddRedisValue<BusinessTechcomBankLoginToken>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BusinessTcbBank, account), token);
        }

        public void RemoveBusinessToken(string account)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.BusinessTcbBank, account));
        }

        public void RemoveToken(string account)
        {
            _redisService.RemoveRedisValue(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.TechcomBank, account));
        }

        public void ReplaceToken(string account, TechcomBankLoginToken token)
        {
            _redisService.AddRedisValue<TechcomBankLoginToken>(CommonHelper.GetBankCacheBankKey(PayMents.PayMentTypeEnum.TechcomBank, account), token);
        }

        public string GetLastRefNoKey(string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.TechcomBank, account));
        }

        public void SetLastRefNoKey(string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.TechcomBank, account), refno);
        }

        public string GetBusinessLastRefNoKey(string account)
        {
            return _redisService.GetRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.BusinessTcbBank, account));
        }

        public void SetBusinessLastRefNoKey(string account, string refno)
        {
            _redisService.AddRedisValue<string>(CommonHelper.GetBankLastRefNoKey(PayMents.PayMentTypeEnum.BusinessTcbBank, account), refno);
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


        //private async Task TestIpAsync(int flag)
        //{
        //    var credentials = new NetworkCredential("aduser", "45686131496");
        //    WebProxy webProxy = new WebProxy(socksUrl, socksPort);
        //    webProxy.BypassProxyOnLocal = true;
        //    webProxy.Credentials = new NetworkCredential(socksusername, sockspassword);
        //    var client = new RestClient(new RestClientOptions("https://ip.ssfox.io/ip")
        //    {
        //        Proxy = webProxy
        //    });

        //    var arrrequest = new RestRequest();
        //    var responseArr = await client.ExecuteGetAsync(arrrequest);
        //    if (flag == 0)
        //    {
        //        NlogLogger.Warn("请求前--获取请求ip:" + responseArr.Content);
        //    }
        //    else
        //    {
        //        NlogLogger.Warn("请求后--获取请求ip:" + responseArr.Content);
        //    }
        //}
    }

}

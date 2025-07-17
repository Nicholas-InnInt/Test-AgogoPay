using Abp.Extensions;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Web.PayMonitorApi.Helpers;
using Neptune.NsPay.Web.PayMonitorApi.Models;
using Neptune.NsPay.Web.PayMonitorApi.Service;
using Newtonsoft.Json;

namespace Neptune.NsPay.Web.PayMonitorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayMonitorController : Controller
    {
        private readonly static string NsPayAll = "NsPayAll";
        private readonly static string Sign = "dcaebf01858f33594ff3074fb7b81d73";
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IMerchantService _merchantService;
        private readonly IAbpUserMerchantService _abpUserMerchantService;
        private readonly IAbpUserService _abpUserService;
        private readonly IRedisService _redisService;
        private readonly IBankStateHelper _bankStateHelper;
        private readonly IPayMonitorCommonHelpers _payMonitorCommonHelpers;
        private readonly IMBBankHelper _mBBankHelper;
        private readonly IVietinBankHelper _vietinBankHelper;
        private readonly ITechcomBankHelper _techcomBankHelper;
        private readonly IVietcomBankHelper _vietcomBankHelper;
        private readonly IBidvBankHelper _bidvBankHelper;
        private readonly IAcbBankHelper _acbBankHelper;
        private readonly IPVcomBankHelper _pVcomBankHelper;
        private readonly IBusinessMBBankHelper _businessMBBankHelper;
        private readonly IBusinessVtbBankHelper _businessVtbBankHelper;
        private readonly IBankHelper _bankHelper;
        private readonly IBankBalanceService _bankBalanceService;
        private readonly IPushUpdateService _pushUpdateService;
        public PayMonitorController(
            IPayGroupMentService payGroupMentService,
            IMerchantService merchantService,
            IAbpUserMerchantService abpUserMerchantService,
            IAbpUserService abpUserService,
            IRedisService redisService,
            IBankStateHelper bankStateHelper,
            IPayMonitorCommonHelpers payMonitorCommonHelpers,
            IMBBankHelper mBBankHelper,
            IVietinBankHelper vietinBankHelper,
            ITechcomBankHelper techcomBankHelper,
            IVietcomBankHelper vietcomBankHelper,
            IBidvBankHelper bidvBankHelper,
            IAcbBankHelper acbBankHelper,
            IPVcomBankHelper pVcomBankHelper,
            IBusinessMBBankHelper businessMBBankHelper,
            IBankBalanceService bankBalanceService,
            IBusinessVtbBankHelper businessVtbBankHelper,
            IBankHelper bankHelper,
            IPushUpdateService pushUpdateService)
        {
            _payGroupMentService = payGroupMentService;
            _merchantService = merchantService;
            _abpUserMerchantService = abpUserMerchantService;
            _abpUserService = abpUserService;
            _redisService = redisService;
            _bankStateHelper = bankStateHelper;
            _payMonitorCommonHelpers = payMonitorCommonHelpers;
            _mBBankHelper = mBBankHelper;
            _vietinBankHelper = vietinBankHelper;
            _techcomBankHelper = techcomBankHelper;
            _vietcomBankHelper = vietcomBankHelper;
            _bidvBankHelper = bidvBankHelper;
            _acbBankHelper = acbBankHelper;
            _pVcomBankHelper = pVcomBankHelper;
            _businessMBBankHelper = businessMBBankHelper;
            _bankBalanceService = bankBalanceService;
            _businessVtbBankHelper = businessVtbBankHelper;
            _bankHelper = bankHelper;
            _pushUpdateService = pushUpdateService;
        }

        [HttpGet]
        public IEnumerable<string> GetTest()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// 检查商户ip地址
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/GetMerchant")]
        public JsonResult GetMerchantLoginIpAddress([FromBody] MerchantInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            }
            if (input.MerchantCode.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });

            if (input.MerchantCode == NsPayAll)
            {
                return new JsonResult(new CheckMerchantResult { Code = StatusCodeEnum.OK, MerchantCode = NsPayAll, LoginIpAddress = "" });
            }
            else
            {
                if (input.MerchantCode == NsPayRedisKeyConst.NsPay)
                {
                    var loginIp = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.IpAddress);
                    if (loginIp != null)
                    {
                        return new JsonResult(new CheckMerchantResult { Code = StatusCodeEnum.OK, MerchantCode = NsPayRedisKeyConst.NsPay, LoginIpAddress = loginIp });
                    }
                    else
                    {
                        return new JsonResult(new CheckMerchantResult { Code = StatusCodeEnum.OK, MerchantCode = NsPayRedisKeyConst.NsPay, LoginIpAddress = "" });
                    }
                }
                else
                {
                    var loginIpAddress = "";
                    var merchant = _redisService.GetMerchantKeyValue(input.MerchantCode);
                    if (merchant == null)
                    {
                        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "商户错误" });
                    }
                    else
                    {
                        var merchantConfig = merchant.MerchantSetting;
                        if (merchantConfig != null)
                        {
                            loginIpAddress = merchantConfig.LoginIpAddress;
                        }
                    }

                    return new JsonResult(new CheckMerchantResult { Code = StatusCodeEnum.OK, MerchantCode = merchant.MerchantCode, LoginIpAddress = loginIpAddress });
                }
            }

        }

        /// <summary>
        /// 获取收款银行
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns> GetPayMent SignalR by status push twice
        [HttpPost]
        [Route("~/PayMonitor/GetPayMent")]
        public async Task<JsonResult> GetPayMents([FromBody] MerchantInput input)
        {
            try
            {
                if (input == null)
                    return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });

                if (input.MerchantCode.IsNullOrEmpty())
                    return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });

                if (input.Sign.ToUpper() != Sign.ToUpper())
                    return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });

                List<PayMentRedisModel> paymentList = new List<PayMentRedisModel>();
                if (input.MerchantCode == NsPayAll)
                {
                    paymentList = _redisService.GetPayMents();
                }
                else if (input.MerchantCode == NsPayRedisKeyConst.NsPay)
                {
                    var groupInfo = _redisService.GetPayGroupMentByGroupName(_redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.NsPayGroupName));
                    if (groupInfo != null)
                    {
                        paymentList = groupInfo.PayMents;
                    }
                }
                else
                {
                    var merchant = _redisService.GetMerchantKeyValue(input.MerchantCode);
                    if (merchant != null)
                    {
                        var groupInfo = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString());
                        if (groupInfo != null)
                        {
                            paymentList = groupInfo.PayMents;
                        }
                    }
                }
                paymentList = paymentList.Where(r => r.Type != PayMentTypeEnum.NoPay && r.Type != PayMentTypeEnum.ScratchCards && r.Type != PayMentTypeEnum.MoMoPay && r.Type != PayMentTypeEnum.ZaloPay && r.DispenseType == PayMentDispensEnum.None && r.ShowStatus == PayMentStatusEnum.Show).ToList();
                List<PayManagerResponseItem> responseItems = new List<PayManagerResponseItem>();
                foreach (var item in paymentList)
                {
                    var payMentBalance = _redisService.GetBalance(item.Id, item.Type);

                    PayManagerResponseItem pay = new PayManagerResponseItem();

                    pay.Id = item.Id;
                    pay.PayType = item.Type.ToString();
                    pay.PayName = item.Name;
                    pay.Name = item.FullName;
                    pay.Phone = item.Phone;
                    pay.BusinessNo = item.Mail;
                    pay.PassWord = item.PassWord;
                    pay.CardNo = item.CardNumber;
                    pay.State = _bankStateHelper.GetPayState(item.CardNumber, item.Type);
                    pay.Account = _bankStateHelper.GetAccount(item.CardNumber, item.Type);
                    pay.Balance = item.Type switch
                    {
                        PayMentTypeEnum.VietcomBank or
                        PayMentTypeEnum.VietinBank or
                        PayMentTypeEnum.BidvBank or
                        PayMentTypeEnum.TechcomBank or
                        PayMentTypeEnum.BusinessTcbBank or
                        PayMentTypeEnum.MsbBank or
                        PayMentTypeEnum.SeaBank or
                        PayMentTypeEnum.BvBank or
                        PayMentTypeEnum.NamaBank or
                        PayMentTypeEnum.TPBank or
                        PayMentTypeEnum.VPBBank or
                        PayMentTypeEnum.OCBBank or
                        PayMentTypeEnum.EXIMBank or
                        PayMentTypeEnum.NCBBank or
                        PayMentTypeEnum.HDBank or
                        PayMentTypeEnum.LPBank or
                        PayMentTypeEnum.PGBank or
                        PayMentTypeEnum.VietBank or
                        PayMentTypeEnum.BacaBank
                          => payMentBalance.Balance2,
                        _ => await _bankBalanceService.GetBalance(item.Id, payMentBalance.Balance, payMentBalance.Balance2, pay.State),
                    };
                    pay.Status = item.UseStatus ? 1 : 0;
                    pay.LimitStatus = _payMonitorCommonHelpers.GetLimitStatus(item.BalanceLimitMoney, pay.Balance);
                    pay.IsUse = _redisService.GetPayUseMent(item.Id) is not null and > 0;

                    responseItems.Add(pay);
                }
                return new JsonResult(new ApiResult<List<PayManagerResponseItem>> { Code = StatusCodeEnum.OK, Message = "", Data = responseItems });
            }
            catch (Exception ex)
            {
                NlogLogger.Error("获取异常：" + ex.ToString());
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "系统异常" });
            }
        }

        [Obsolete]
        /// <summary>
        /// 检查提现银行
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/GetWithdrawPayMent")]
        public async Task<JsonResult> GetWithdrawPayMents([FromBody] MerchantInput input)
        {
            if (input == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            }
            if (input.MerchantCode.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });
            }
            if (input.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });

            List<PayMentRedisModel> paymentList = new List<PayMentRedisModel>();
            if (input.MerchantCode == NsPayAll)
            {
                paymentList = _redisService.GetPayMents();
            }
            else
            {
                if (input.MerchantCode == NsPayRedisKeyConst.NsPay)
                {
                    var groupInfo = _redisService.GetPayGroupMentByGroupName(_redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.NsPayGroupName));
                    if (groupInfo != null)
                    {
                        paymentList = groupInfo.PayMents;
                    }
                }
                else
                {
                    var merchant = _redisService.GetMerchantKeyValue(input.MerchantCode);
                    if (merchant != null)
                    {
                        var groupInfo = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString());
                        if (groupInfo != null)
                        {
                            paymentList = groupInfo.PayMents;
                        }
                    }
                }
            }
            paymentList = paymentList.Where(r => r.Type != PayMentTypeEnum.NoPay && r.Type != PayMentTypeEnum.ScratchCards && r.Type != PayMentTypeEnum.MoMoPay && r.Type != PayMentTypeEnum.ZaloPay && r.DispenseType != PayMentDispensEnum.None && r.ShowStatus == PayMentStatusEnum.Show).ToList();
            List<PayManagerResponseItem> responseItems = new List<PayManagerResponseItem>();
            foreach (var item in paymentList)
            {
                PayManagerResponseItem pay = new PayManagerResponseItem();
                pay.Id = item.Id;
                pay.PayType = item.Type.ToString();
                pay.PayName = item.Name;
                pay.Name = item.FullName;
                pay.Phone = item.Phone;
                pay.BusinessNo = item.Mail;
                pay.PassWord = item.PassWord;
                pay.CardNo = item.CardNumber;
                //pay.Balance = _redisService.GetBalance(item.Id, item.Type);
                //pay.State = _bankStateHelper.GetPayState(item.Phone, item.Type);
                pay.State = _bankStateHelper.GetPayState(item.Phone, item.Type);
                var payMentBalance = _redisService.GetBalance(item.Id, item.Type);
                pay.Balance = await _bankBalanceService.GetBalance(item.Id, payMentBalance.Balance, payMentBalance.Balance2, pay.State);
                pay.Status = item.UseStatus ? 1 : 0;
                pay.LimitStatus = _payMonitorCommonHelpers.GetLimitStatus(item.BalanceLimitMoney, pay.Balance);
                var isuse = _redisService.GetPayUseMent(item.Id);
                pay.IsUse = (isuse == null || isuse <= 0) ? false : true;
                responseItems.Add(pay);
            }
            return new JsonResult(new ApiResult<List<PayManagerResponseItem>> { Code = StatusCodeEnum.OK, Message = "", Data = responseItems });
        }

        /// <summary>
        /// 设置登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/SetPaySession")]
        public async Task<JsonResult> SetPaySession([FromBody] PaySessionRequest request)
        {
            NlogLogger.Info("SetPaySession：" + request.ToJsonString());

            bool haveSessionSet = false;
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.PayType.IsNullOrEmpty() || request.Phone.IsNullOrEmpty() || request.Sign.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == request.PayMentId && r.IsDeleted == false);
            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (payment.PassWord.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "密码错误" });

            var beforeLoginState = _bankStateHelper.GetPayState(payment.CardNumber, payment.Type);

            if (payment.Type == PayMentTypeEnum.MBBank)
            {
                MbBankLoginModel mbBankLogin = new MbBankLoginModel()
                {
                    SessionId = request.Code,
                    ErrorCount = 0
                };
                _mBBankHelper.SetSessionId(payment.CardNumber, mbBankLogin);
                haveSessionSet = true;
            }
            else if (payment.Type == PayMentTypeEnum.VietinBank)
            {
                //string sessonid = _vietinBankHelper.GetLogin(payment.Phone, payment.PassWord, request.Code, request.CaptchaId);
                //if (string.IsNullOrEmpty(sessonid))
                //    return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "登录失败" });
                _vietinBankHelper.SetSessionId(payment.CardNumber, new VtbBankLoginModel()
                {
                    Account = request.Account,
                    sessionid = request.SessionId ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
                haveSessionSet = true;
            }
            else if (payment.Type == PayMentTypeEnum.TechcomBank)
            {
                if (!request.TechcomCookie.IsNullOrEmpty())
                {
                    var cookie = request.TechcomCookie.FromJsonString<TechcomBankLoginToken>();
                    if (cookie != null)
                    {
                        cookie.CreateTime = DateTime.Now;
                        cookie.Account = request.Account;
                        _techcomBankHelper.SetToken(payment.CardNumber, cookie);
                        haveSessionSet = true;
                    }
                }
            }
            else if (payment.Type == PayMentTypeEnum.VietcomBank)
            {
                _vietcomBankHelper.SetSessionId(payment.CardNumber, new VietcomBankLoginResponse()
                {
                    Account = request.Account,
                    CreateTime = DateTime.Now,
                });
                haveSessionSet = true;
                //var bankUrl = _redisService.GetPayGroupMentByPayMentId(payment.Id)?.VietcomApi;
                //if (bankUrl != null)
                //{
                //    VietcomLoginRequest loginRequest = new VietcomLoginRequest()
                //    {
                //        username = request.Phone,
                //        password = payment.PassWord,
                //        captchaValue = request.Code,
                //        captchaToken = request.CaptchaId
                //    };
                //    var response = await _vietcomBankHelper.GetFirstLoginAsync(loginRequest, payment.CardNumber,bankUrl);
                //    if (response != null)
                //    {
                //        //VietcomBank第一步登录完成，返回成功
                //        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = response.loginCache.challenge });
                //    }
                //    else
                //    {
                //        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "登录失败" });
                //    }
                //}
                //else
                //{
                //    return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "登录失败" });
                //}
            }
            else if (payment.Type == PayMentTypeEnum.BidvBank)
            {
                _bidvBankHelper.SetSessionId(payment.CardNumber, new BidvBankLoginResponse()
                {
                    Account = request.Account,
                    Createtime = DateTime.Now,
                    paySessionModel = JsonConvert.DeserializeObject<PaySessionBidvModel>(request.BIDVSessionData),
                }
                );

                haveSessionSet = true;

                //var bankUrl = _redisService.GetPayGroupMentByPayMentId(payment.Id)?.BankApi;
                //if (bankUrl != null)
                //{
                //    var response = await _bidvBankHelper.GetFirstLogin(payment.CardNumber, request.Phone, payment.PassWord, request.Code, request.CaptchaId, bankUrl);
                //    if (response != null)
                //    {
                //        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
                //    }
                //    else
                //    {
                //        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "登录失败" });
                //    }
                //}
                //else
                //{
                //    return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "登录失败" });
                //}
            }
            else if (payment.Type == PayMentTypeEnum.ACBBank)
            {
                int type = 0;
                if (payment.Mail == "businessacb")
                {
                    type = 1;
                }
                //var bankUrl = _redisService.GetPayGroupMentByPayMentId(payment.Id)?.BankApi;
                var bankUrl = _redisService.GetPayGroupMentByPayMentId(payment.Id)?.BankApi;
                if (payment.BusinessType == true && payment.Type == PayMentTypeEnum.ACBBank)
                {
                    bankUrl = "http://bank2.nsmanage.top/";
                }
                if (bankUrl != null)
                {
                    var response = await _acbBankHelper.GetLoginAsync(request.Phone, payment.CardNumber, request.CaptchaId, bankUrl, type);
                    if (response != null)
                    {
                        haveSessionSet = true;
                    }
                }
            }
            else if (payment.Type == PayMentTypeEnum.PVcomBank)
            {
                if (!request.TechcomCookie.IsNullOrEmpty())
                {
                    var cookie = request.TechcomCookie.FromJsonString<PVcomBankLoginModel>();
                    if (cookie != null)
                    {
                        cookie.CreateTime = DateTime.Now;
                        _pVcomBankHelper.SetSessionId(payment.CardNumber, cookie);
                        haveSessionSet = true;
                    }
                }
            }
            else if (payment.Type == PayMentTypeEnum.BusinessMbBank)
            {
                if (!request.TechcomCookie.IsNullOrEmpty())
                {
                    var cookie = request.TechcomCookie.FromJsonString<BusinessMBBankLoginModel>();
                    if (cookie != null)
                    {
                        cookie.CreateTime = DateTime.Now;
                        _businessMBBankHelper.SetSessionId(payment.CardNumber, cookie);
                        haveSessionSet = true;
                    }
                }
            }
            else if (payment.Type == PayMentTypeEnum.BusinessTcbBank)
            {
                if (!request.TechcomCookie.IsNullOrEmpty())
                {
                    //var cookie = request.TechcomCookie.FromJsonString<BusinessTechcomBankLoginToken>();
                    var cookie = JsonConvert.DeserializeObject<BusinessTechcomBankLoginToken>(request.TechcomCookie);
                    if (cookie != null)
                    {
                        cookie.CreateTime = DateTime.Now;
                        cookie.Account = request.Account;
                        _techcomBankHelper.SetBusinessToken(payment.CardNumber, cookie);
                        haveSessionSet = true;
                    }
                }
            }
            else if (payment.Type == PayMentTypeEnum.BusinessVtbBank)
            {
                if (!request.TechcomCookie.IsNullOrEmpty())
                {
                    var cookie = JsonConvert.DeserializeObject<BusinessVtbBankLoginModel>(request.TechcomCookie);
                    if (cookie != null)
                    {
                        cookie.CreateTime = DateTime.Now;
                        _businessVtbBankHelper.SetSessionId(payment.CardNumber, cookie);
                        haveSessionSet = true;
                    }
                }
            }
            else
            {
                _bankHelper.SetBankSessionId(payment.Type, payment.CardNumber, new BankLoginModel()
                {
                    Account = request.Account,
                    Createtime = DateTime.Now,
                    Token = request.Token,
                    Cookies = request.Cookies,
                });
                haveSessionSet = true;
            }

            if (haveSessionSet)
            {
                if (beforeLoginState == 0) // meaning before add is offline
                {
                    _payMonitorCommonHelpers.NotifyMerchantPaymentChanged(payment.Id);
                }

                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
            }

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "登录失败" });
        }

        /// <summary>
        /// 设置登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/RemovePaySession")]
        public async Task<JsonResult> RemovePaySession([FromBody] PaySessionRequest request)
        {
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.PayType.IsNullOrEmpty() || request.Phone.IsNullOrEmpty() || request.Sign.IsNullOrEmpty() || request.Account.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == request.PayMentId);
            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });

            bool haveSessionRemoved = false;
            //if (payment.Type == PayMentTypeEnum.MBBank)
            //{
            //    _mBBankHelper.RemoveSessionId(payment.CardNumber);
            //}
            if (payment.Type == PayMentTypeEnum.VietinBank)
            {
                var cacheToken = _vietinBankHelper.GetVtbBankLoginModel(payment.CardNumber);
                if (cacheToken != null)
                {
                    if (cacheToken.Account == request.Account)
                    {
                        _vietinBankHelper.RemoveSessionId(payment.CardNumber);
                        haveSessionRemoved = true;
                    }
                }
            }
            else if (payment.Type == PayMentTypeEnum.TechcomBank)
            {
                var cacheToken = _techcomBankHelper.GetToken(payment.CardNumber);
                if (cacheToken != null)
                {
                    if (cacheToken.Account == request.Account)
                    {
                        _techcomBankHelper.RemoveToken(payment.CardNumber);
                        haveSessionRemoved = true;
                    }
                }
            }
            else if (payment.Type == PayMentTypeEnum.VietcomBank)
            {
                var cacheToken = _vietcomBankHelper.GetSessionId(payment.CardNumber);
                if (cacheToken != null)
                {
                    if (cacheToken.Account == request.Account)
                    {
                        _vietcomBankHelper.RemoveSessionId(payment.CardNumber);
                        haveSessionRemoved = true;
                    }
                }
            }
            else if (payment.Type == PayMentTypeEnum.BidvBank)
            {
                var cacheToken = _bidvBankHelper.GetSessionId(payment.CardNumber);
                if (cacheToken != null)
                {
                    if (cacheToken.Account == request.Account)
                    {
                        _bidvBankHelper.RemoveSessionId(payment.CardNumber);
                        haveSessionRemoved = true;
                    }
                }
            }
            //if (payment.Type == PayMentTypeEnum.ACBBank)
            //{
            //    _acbBankHelper.RemoveSessionId(payment.CardNumber);
            //}
            //if (payment.Type == PayMentTypeEnum.PVcomBank)
            //{
            //    _pVcomBankHelper.RemoveSessionId(payment.CardNumber);
            //}
            //if (payment.Type == PayMentTypeEnum.BusinessMbBank)
            //{
            //    _businessMBBankHelper.RemoveToken(payment.CardNumber);
            //}
            else if (payment.Type == PayMentTypeEnum.BusinessTcbBank)
            {
                var cacheToken = _techcomBankHelper.GetToken(payment.CardNumber);
                if (cacheToken != null)
                {
                    if (cacheToken.Account == request.Account)
                    {
                        _techcomBankHelper.RemoveToken(payment.CardNumber);
                        haveSessionRemoved = true;
                    }
                }
            }
            else
            {
                var cacheToken = _bankHelper.GetBankSessionId(payment.Type, payment.CardNumber);
                if (cacheToken != null)
                {
                    if (cacheToken.Account == request.Account)
                    {
                        _bankHelper.RemoveBankSessionId(payment.Type, payment.CardNumber);
                        haveSessionRemoved = true;
                    }
                }
            }

            if (haveSessionRemoved)
            {
                await _payMonitorCommonHelpers.NotifyMerchantPaymentChanged(payment.Id);
            }

            //if (payment.Type == PayMentTypeEnum.BusinessVtbBank)
            //{
            //    _businessVtbBankHelper.RemoveToken(payment.CardNumber);
            //}
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// bidv和vcb需要二步登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/SetSecondLogin")]
        public async Task<JsonResult> SetSecondLogin([FromBody] PaySessionRequest request)
        {
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.PayType.IsNullOrEmpty() || request.Phone.IsNullOrEmpty() || request.Sign.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == request.PayMentId);
            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (payment.PassWord.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "密码错误" });
            if (payment.Type == PayMentTypeEnum.VietcomBank)
            {
                var bankUrl = _redisService.GetPayGroupMentByPayMentId(payment.Id)?.VietcomApi;
                if (bankUrl != null)
                {
                    VietcomLoginRequest loginRequest = new VietcomLoginRequest()
                    {
                        username = payment.Phone,
                        codeotp = request.Code
                    };
                    var response = await _vietcomBankHelper.GetSecondLoginAsync(loginRequest, payment.CardNumber, bankUrl);
                    if (response != null)
                    {
                        _vietcomBankHelper.SetSessionId(payment.CardNumber, response);
                        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
                    }
                    else
                    {
                        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "登录失败" });
                    }
                }
            }
            if (payment.Type == PayMentTypeEnum.BidvBank)
            {
                var bankUrl = _redisService.GetPayGroupMentByPayMentId(payment.Id)?.BankApi;
                if (bankUrl != null)
                {
                    var response = await _bidvBankHelper.GetSecondLoginAsync(payment.CardNumber, request.Phone, request.CaptchaId, bankUrl);
                    if (response != null)
                    {
                        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
                    }
                    else
                    {
                        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "登录失败" });
                    }
                }
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "" });
        }

        /// <summary>
        /// 设置余额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/SetBalance")]
        public async Task<JsonResult> SetBalance([FromBody] BalanceInput request)
        {
            NlogLogger.Trace("余额更新：" + request.ToJsonString());
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });
            if (request.PayMentId <= 0)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == request.PayMentId);
            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            var checkBalance = _redisService.GetBalanceByPaymentId(request.PayMentId);
            if (checkBalance != null)
            {
                checkBalance.Balance2 = request.Balance;
                checkBalance.UpdateTime = DateTime.Now;
                _redisService.SetBalance(payment.Id, checkBalance);
                await _pushUpdateService.BalanceChanged(new BalanceUpdateNotification() { PayMentId = checkBalance.PayMentId, Balance = checkBalance.Balance2, UpdatedTime = checkBalance.UpdateTime });

            }
            else
            {
                var balance = new BankBalanceModel()
                {
                    PayMentId = payment.Id,
                    Type = payment.Type,
                    UserName = payment.Phone,
                    Balance = 0,
                    Balance2 = request.Balance,
                    UpdateTime = DateTime.Now
                };
                _redisService.SetBalance(payment.Id, balance);
                await _pushUpdateService.BalanceChanged(new BalanceUpdateNotification() { PayMentId = balance.PayMentId, Balance = balance.Balance2, UpdatedTime = balance.UpdateTime });
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 设置启用
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/SetStatus")]
        public async Task<JsonResult> SetStatus([FromBody] PayMonitorStatusInput request)
        {
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            NlogLogger.Fatal("账户：" + request.Account + "参数：" + request.ToJsonString() + "操作时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });
            if (request.PayMentId <= 0)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });

            if (request.MerchantCode.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == request.PayMentId);
            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });

            int payGroupId = -1;

            if (request.MerchantCode == NsPayRedisKeyConst.NsPay || request.MerchantCode == NsPayAll)
            {
                var groupInfo = _redisService.GetPayGroupMentByGroupName(_redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.NsPayGroupName));

                if (groupInfo != null)
                {
                    payGroupId = groupInfo.GroupId;
                }
            }
            else
            {
                var merchant = _redisService.GetMerchantKeyValue(request.MerchantCode);
                if (merchant == null)
                {
                    return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "商户错误" });
                }
                else
                {
                    payGroupId = merchant.PayGroupId;
                }

            }

            if (payGroupId == -1)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "商户错误" });
            }

            var cacheInfo = _redisService.GetPayGroupMentRedisValue(payGroupId.ToString());
            if (cacheInfo != null)
            {
                var paygroupment = await _payGroupMentService.GetFirstAsync(r => r.PayMentId == payment.Id && r.GroupId == payGroupId && r.IsDeleted == false);
                if (paygroupment != null)
                {
                    paygroupment.Status = Convert.ToBoolean(request.Status);
                    await _payGroupMentService.UpdateAsync(paygroupment);
                    if (paygroupment.Status)
                    {
                        //更新缓存
                        var info = cacheInfo.PayMents.FirstOrDefault(r => r.Id == payment.Id);
                        if (info != null)
                        {
                            cacheInfo.PayMents.Remove(info);
                            info.UseStatus = true;
                            cacheInfo.PayMents.Add(info);
                            _redisService.AddPayGroupMentRedisValue(cacheInfo.GroupId.ToString(), cacheInfo);
                        }

                        //加入正在使用缓存
                        _redisService.SetPayUseMent(payment.Id);
                    }
                    else
                    {
                        //更新缓存
                        var info = cacheInfo.PayMents.FirstOrDefault(r => r.Id == payment.Id);
                        if (info != null)
                        {
                            cacheInfo.PayMents.Remove(info);
                            info.UseStatus = false;
                            cacheInfo.PayMents.Add(info);
                            _redisService.AddPayGroupMentRedisValue(cacheInfo.GroupId.ToString(), cacheInfo);
                        }
                    }
                    await _payMonitorCommonHelpers.NotifyMerchantPaymentChanged(payment.Id);

                }
            }
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 检查账户名
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/CheckAccount")]
        public async Task<JsonResult> CheckAccount([FromBody] PayMonitorAccountInput request)
        {
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });
            if (request.Account.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });

            //检查商户号
            var merchantCode = "";
            var merchantId = 0;
            Merchant? merchant = null;
            //检测商户是否正常
            if (request.MerchantCode != "NsPay")
            {
                merchant = await _merchantService.GetFirstAsync(r => r.MerchantCode == request.MerchantCode);
                if (merchant != null)
                {
                    merchantCode = merchant.MerchantCode;
                    merchantId = merchant.Id;
                }
            }
            else
            {
                merchantCode = request.MerchantCode;
            }
            if (merchantCode.IsNullOrEmpty())
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "商户错误" });
            }

            //检查账户
            var checkInfo = await _abpUserService.GetFirstAsync(r => r.UserName == request.Account && r.IsDeleted == false);
            if (checkInfo != null)
            {
                //检查是否关联
                if (merchantCode != "NsPay")
                {
                    if (merchant != null)
                    {
                        if (merchant.MerchantType == MerchantTypeEnum.Internal)
                        {
                            var usermerchant = await _abpUserMerchantService.GetFirstAsync(r => r.UserId == checkInfo.Id && r.MerchantId == merchantId);
                            if (usermerchant != null)
                            {
                                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
                            }
                            else
                            {
                                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "账户错误" });
                            }
                        }
                        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "账户错误" });
                    }
                    else
                    {
                        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "账户错误" });
                    }
                }
                else
                {
                    if (checkInfo.UserType == UserTypeEnum.NsPayKefu || checkInfo.UserType == UserTypeEnum.NsPayAdmin)
                    {
                        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
                    }
                    else
                    {
                        return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "账户错误" });
                    }
                }
            }
            else
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "账户错误" });
            }
        }

        /// <summary>
        /// 清楚正在使用
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/ClearUse")]
        public async Task<JsonResult> ClearUse([FromBody] PayMonitorClearUseInput request)
        {
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });
            if (request.PayMentId <= 0)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "参数错误" });

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == request.PayMentId);
            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            _redisService.RemovePayUseMent(payment.Id);
            await _payMonitorCommonHelpers.NotifyMerchantPaymentChanged(payment.Id);
            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        [HttpGet]
        [Route("~/PayMonitor/GetVersion")]
        public string GetVersion()
        {
            var str = AppSettings.Configuration["Version"];
            return str;
        }

        /// <summary>
        /// 添加入款账户登陆记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/SetAccountLoginAction")]
        public async Task<JsonResult> SetAccountLogin([FromBody] SetAccountLoginRequest request)
        {
            NlogLogger.Info("SetAccountLogin：" + request.ToJsonString());
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.PayMentId <= 0 || request.Sign.IsNullOrEmpty() || request.MerchantCode.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == request.PayMentId && r.IsDeleted == false);
            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });

            #region add to redis

            var redisModel = _payMonitorCommonHelpers.Mapper().Map<AccountLoginActionRedisModel>(request);
            redisModel.TypeStr = payment.Type.ToString();
            redisModel.Phone = payment.Phone;
            redisModel.Account = payment.CardNumber;
            _redisService.SetAccountLoginAction(payment.CardNumber, payment.Type, request.MerchantCode, redisModel);

            #endregion

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });

        }


        /// <summary>
        /// 获取入款账户登陆记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/GetAccountLoginAction")]
        public async Task<JsonResult> GetAccountLogin([FromBody] GetAccountLoginRequest request)
        {
            NlogLogger.Info("GetAccountLogin：" + request.ToJsonString());
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Account.IsNullOrEmpty() || request.Phone.IsNullOrEmpty() || request.TypeStr.IsNullOrEmpty() || request.MerchantCode.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });


            var actualPaymentType = Enum.GetValues(typeof(PayMentTypeEnum))
                            .Cast<PayMentTypeEnum>()
                            .Select(status => new { Text = status.ToString().ToLower(), actual = status }).FirstOrDefault(x => x.Text == request.TypeStr.ToLower());

            if (actualPaymentType == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            }

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Phone == request.Phone && r.Type == actualPaymentType.actual && r.CardNumber == request.Account && r.IsDeleted == false);

            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });

            #region Get From redis

            var loginActionModel = _redisService.GetAccountLoginAction(payment.CardNumber, payment.Type, request.MerchantCode);

            if (loginActionModel != null) // Existing In Cache
            {
                return new JsonResult(new ApiResult<AccountLoginActionRedisModel> { Code = StatusCodeEnum.OK, Message = "", Data = loginActionModel });
            }
            else
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "账号未登录" });
            }

            #endregion
        }

        /// <summary>
        /// 移除入款账户登陆记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/RemoveAccountLoginAction")]
        public async Task<JsonResult> RemoveAccountLogin([FromBody] RemoveAccountLoginRequest request)
        {
            NlogLogger.Info("RemoveAccountLoginAction：" + request.ToJsonString());
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.PayMentId <= 0 || request.Sign.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == request.PayMentId && r.IsDeleted == false);
            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });

            #region remove from redis

            _redisService.RemoveAccountLoginAction(payment.CardNumber, payment.Type, request.MerchantCode);

            #endregion

            return new JsonResult(new BaseReponse { Code = StatusCodeEnum.OK, Message = "" });
        }

        /// <summary>
        /// 获取入款账户登陆记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/PayMonitor/GetAccountLoginActionWeb")]
        public async Task<JsonResult> GetAccountLoginWeb([FromBody] GetAccountLoginWebRequest request)
        {
            NlogLogger.Info("GetAccountLoginActionWeb：" + request.ToJsonString());
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.PayMentId <= 0 || request.Sign.IsNullOrEmpty() || request.MerchantCode.IsNullOrEmpty())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Sign.ToUpper() != Sign.ToUpper())
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "签名错误" });

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == request.PayMentId && r.IsDeleted == false);

            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });

            #region Get From redis

            var loginActionModel = _redisService.GetAccountLoginAction(payment.CardNumber, payment.Type, request.MerchantCode);

            if (loginActionModel != null) // Existing In Cache
            {
                return new JsonResult(new ApiResult<AccountLoginActionRedisModel> { Code = StatusCodeEnum.OK, Message = "", Data = loginActionModel });
            }
            else
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "账号未登录" });
            }

            #endregion
        }

        [HttpPost]
        [Route("~/PayMonitor/UpdateAccountLoginActionStatus")]
        public async Task<JsonResult> UpdateAccountLoginStatus([FromBody] UpdateAccountLoginStatusRequest request)
        {
            NlogLogger.Info("UpdateAccountLoginStatus：" + request.ToJsonString());
            if (request == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            if (request.Account.IsNullOrEmpty() || request.TypeStr.IsNullOrEmpty() | request.Phone.IsNullOrEmpty() || request.Status < 0)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });

            var actualPaymentType = Enum.GetValues(typeof(PayMentTypeEnum))
                         .Cast<PayMentTypeEnum>()
                         .Select(status => new { Text = status.ToString().ToLower(), actual = status }).FirstOrDefault(x => x.Text == request.TypeStr.ToLower());

            if (actualPaymentType == null)
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });
            }

            var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Phone == request.Phone && r.Type == actualPaymentType.actual && r.CardNumber == request.Account && r.IsDeleted == false);

            if (payment == null)
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "请求错误" });

            #region update into redis


            var loginActionModel = _redisService.GetAccountLoginAction(payment.CardNumber, payment.Type, request.MerchantCode);

            if (loginActionModel != null) // Existing In Cache
            {
                loginActionModel.LoginOTP = request.OTP;
                loginActionModel.Phone = payment.Phone;
                loginActionModel.Account = payment.CardNumber;
                loginActionModel.Status = request.Status;

                _redisService.SetAccountLoginAction(payment.CardNumber, payment.Type, request.MerchantCode, loginActionModel);
                return new JsonResult(new ApiResult<AccountLoginActionRedisModel> { Code = StatusCodeEnum.OK, Message = "", Data = loginActionModel });
            }
            else
            {
                return new JsonResult(new BaseReponse { Code = StatusCodeEnum.ERROR, Message = "账号未登录" });
            }

            #endregion
        }
    }
}
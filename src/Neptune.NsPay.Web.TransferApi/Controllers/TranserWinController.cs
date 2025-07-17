using Abp.Collections.Extensions;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Web.TransferApi.Models;
using Stripe;
using Twilio.TwiML.Voice;

namespace Neptune.NsPay.Web.TransferApi.Controllers
{
    public class TranserWinController : ControllerBase
    {
        private readonly static string NsPayAll = "NsPayAll";
        private readonly static string Sign = "b94415291da04406b11c2d6a9d9a7342";
        private readonly IMerchantService _merchantService;
        private readonly IAbpUserMerchantService _abpUserMerchantService;
        private readonly IAbpUserService _abpUserService;
        private readonly IRedisService _redisService;
        private readonly IWithdrawalDevicesService _withdrawalDevicesService;

        public TranserWinController(IMerchantService merchantService,
            IAbpUserMerchantService abpUserMerchantService,
            IAbpUserService abpUserService,
            IRedisService redisService,
            IWithdrawalDevicesService withdrawalDevicesService)
        {
            _merchantService = merchantService;
            _abpUserMerchantService = abpUserMerchantService;
            _abpUserService = abpUserService;
            _redisService = redisService;
            _withdrawalDevicesService = withdrawalDevicesService;
        }

        /// <summary>
        /// 检查商户ip地址
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/TransferWin/GetMerchant")]
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
        /// 检查提现银行
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/TransferWin/GetWithdrawDevices")]
        public JsonResult GetWithdrawDevices([FromBody] MerchantInput input)
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

            List<WithdrawalDeviceRedisModel> withdrawList = new List<WithdrawalDeviceRedisModel>();
            if (input.MerchantCode == NsPayRedisKeyConst.NsPay)
            {
                input.MerchantCode= NsPayRedisKeyConst.NsPay;
            }
            var list = _withdrawalDevicesService.GetAll().Where(r => r.MerchantCode == input.MerchantCode && r.IsDeleted == false);
            foreach (var device in list) 
            {
                var balanceInfo = _redisService.GetWitdrawDeviceBalance(device.Id);
                WithdrawalDeviceRedisModel redisModel = new WithdrawalDeviceRedisModel()
                {
                    Id = device.Id,
                    MerchantCode = device.MerchantCode,
                    Name = device.Name,
                    Phone = device.Phone,
                    BankOtp = device.BankOtp,
                    BankType = device.BankType,
                    CardName = device.CardName,
                    LoginPassWord = device.LoginPassWord,
                    Process = device.Process,
                    Status = true,
                    DeviceAdbName = device.DeviceAdbName,
                    Balance = balanceInfo == null ? 0 : balanceInfo.Balance
                };
                withdrawList.Add(redisModel);
            }
            return new JsonResult(new ApiResult<List<WithdrawalDeviceRedisModel>> { Code = StatusCodeEnum.OK, Message = "", Data = withdrawList });
        }

        /// <summary>
        /// 检查账户名
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/TransferWin/CheckAccount")]
        public async Task<JsonResult> CheckAccount([FromBody] TransferWinAccountInput request)
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

        [HttpGet]
        [Route("~/TransferWin/GetVersion")]
        public string GetVersion()
        {
            var str = AppSettings.Configuration["Version"];
            return str;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Web.Api.Controllers;
using Neptune.NsPay.Web.Api.Models;

namespace Neptune.NsPay.Web.PayMonitorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WithdrawalController : BaseController
    {
        private readonly IRedisService _redisService;
        private readonly IWithdrawalDevicesService _withdrawalDevicesService;
        public WithdrawalController(IRedisService redisService,
            IWithdrawalDevicesService withdrawalDevicesService)
        {
            _redisService = redisService;
            _withdrawalDevicesService = withdrawalDevicesService;
        }

        [Route("~/Withdrawal/GetWithdrawalInfo")]
        [HttpPost]
        public async Task<JsonResult> GetWithdrawalInfo([FromForm] string name)
        {
            if (string.IsNullOrEmpty(name))
                return toResponseError(StatusCodeType.Error, "");
            var info = await _withdrawalDevicesService.GetFirstAsync(r => r.Name == name && r.Status == true);
            if (info == null)
                return toResponseError(StatusCodeType.Error, "");
            if (info != null)
            {
                WithdrawalDeviceViewDto viewDto = new WithdrawalDeviceViewDto()
                {
                    Id = info.Id,
                    Name = info.Name,
                    Phone = info.Phone,
                    Status = info.Status,
                    Process = (int)info.Process,
                    LoginPassWord = info.LoginPassWord,
                    BankOtp = GetBankOtp(info.BankOtp),
                };
                return toResponse(viewDto);
            }
            return toResponse(StatusCodeType.Success, "");
        }

        [Route("~/Withdrawal/UpdateTcbOrder")]
        [HttpPost]
        public JsonResult UpdateTcbOrder([FromForm] string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return toResponseError(StatusCodeType.Error, "");

            var info = _redisService.GeTcbTransferOrder(phone);
            if (info == null)
                return toResponseError(StatusCodeType.Error, "");
            info.Status = 1;
            info.UpdateTime = DateTime.Now;
            _redisService.SetTcbTransferOrder(phone, info);
            return toResponse(info);
        }

        [Route("~/Withdrawal/GetVcbOrder")]
        [HttpPost]
        public JsonResult GetVcbOrder([FromForm] string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return toResponseError(StatusCodeType.Error, "");

            var info = _redisService.GetVcbTransferOrder(phone);
            if (info == null)
                return toResponseError(StatusCodeType.Error, "");
            if (string.IsNullOrEmpty(info.SmartOtp))
            {
                info.SmartOtp = "";
            }
            return toResponse(info);
        }

        [Route("~/Withdrawal/UpdateVcbOrder")]
        [HttpPost]
        public JsonResult UpdateVcbOrder([FromForm] GetVcbOrderInput input)
        {
            if (input == null)
                return toResponseError(StatusCodeType.Error, "");
            if (string.IsNullOrEmpty(input.Phone))
                return toResponseError(StatusCodeType.Error, "");

            var info = _redisService.GetVcbTransferOrder(input.Phone);
            if (info == null)
                return toResponseError(StatusCodeType.Error, "");
            if (info.TranxId == info.TranxId)
            {
                info.SmartOtp = input.SmartOtp;
                info.UpdateTime = DateTime.Now;
                _redisService.SetVcbTransferOrder(input.Phone, info);
                return toResponse(info);
            }
            else
            {
                return toResponseError(StatusCodeType.Error, "");
            }
        }

        private List<string> GetBankOtp(string bankotp)
        {
            var bank = new List<string>();
            var charArr = bankotp.ToCharArray();
            foreach (var item in charArr)
            {
                bank.Add(item.ToString());
            }
            return bank;
        }
    }
}

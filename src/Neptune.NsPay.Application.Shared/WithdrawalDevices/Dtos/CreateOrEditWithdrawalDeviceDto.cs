using Neptune.NsPay.WithdrawalDevices;

using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.WithdrawalDevices.Dtos
{
    public class CreateOrEditWithdrawalDeviceDto : EntityDto<int?>
    {

        [StringLength(WithdrawalDeviceConsts.MaxNameLength, MinimumLength = WithdrawalDeviceConsts.MinNameLength)]
        public string Name { get; set; }

        [StringLength(WithdrawalDeviceConsts.MaxPhoneLength, MinimumLength = WithdrawalDeviceConsts.MinPhoneLength)]
        public string Phone { get; set; }

        public string CardNumber { get; set; }

        public WithdrawalDevicesBankTypeEnum BankType { get; set; }

        [StringLength(WithdrawalDeviceConsts.MaxBankOtpLength, MinimumLength = WithdrawalDeviceConsts.MinBankOtpLength)]
        public string BankOtp { get; set; }

        [StringLength(WithdrawalDeviceConsts.MaxLoginPassWordLength, MinimumLength = WithdrawalDeviceConsts.MinLoginPassWordLength)]
        public string LoginPassWord { get; set; }

        public string MerchantCode { get; set; }

        public WithdrawalDevicesProcessTypeEnum Process { get; set; }

        public string DeviceAdbName { get; set; }

        public decimal MinMoney { get; set; }

        public decimal MaxMoney { get; set; }

        //public bool Status { get; set; }

    }
}
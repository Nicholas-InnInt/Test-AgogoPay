using Neptune.NsPay.WithdrawalDevices;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Neptune.NsPay.WithdrawalDevices
{
    [Table("WithdrawalDevices")]
    public class WithdrawalDevice : FullAuditedEntity
    {

        [StringLength(WithdrawalDeviceConsts.MaxNameLength, MinimumLength = WithdrawalDeviceConsts.MinNameLength)]
        public virtual string Name { get; set; }

        [StringLength(WithdrawalDeviceConsts.MaxPhoneLength, MinimumLength = WithdrawalDeviceConsts.MinPhoneLength)]
        public virtual string Phone { get; set; }

        [StringLength(WithdrawalDeviceConsts.MaxBankOtpLength, MinimumLength = WithdrawalDeviceConsts.MinBankOtpLength)]
        public virtual string CardNumber { get; set; }

        public virtual WithdrawalDevicesBankTypeEnum BankType { get; set; }

        [StringLength(WithdrawalDeviceConsts.MaxBankOtpLength, MinimumLength = WithdrawalDeviceConsts.MinBankOtpLength)]
        public virtual string BankOtp { get; set; }

        [StringLength(WithdrawalDeviceConsts.MaxLoginPassWordLength, MinimumLength = WithdrawalDeviceConsts.MinLoginPassWordLength)]
        public virtual string LoginPassWord { get; set; }

        [StringLength(WithdrawalDeviceConsts.MaxCardNameLength, MinimumLength = WithdrawalDeviceConsts.MinCardNameLength)]
        public virtual string CardName { get; set; }

        [StringLength(WithdrawalDeviceConsts.MaxMerchantCodeLength, MinimumLength = WithdrawalDeviceConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        public virtual WithdrawalDevicesProcessTypeEnum Process { get; set; }

        public virtual bool Status { get; set; }

        [StringLength(WithdrawalDeviceConsts.MaxMerchantCodeLength, MinimumLength = WithdrawalDeviceConsts.MinMerchantCodeLength)]
        public virtual string DeviceAdbName { get; set; }

        public virtual decimal MinMoney { get; set; }

        public virtual decimal MaxMoney { get; set; }
    }
}
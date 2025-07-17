using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Neptune.NsPay.MerchantSettings
{
    [Table("MerchantSettings")]
    public class MerchantSetting : FullAuditedEntity
    {

        [StringLength(MerchantSettingConsts.MaxMerchantCodeLength, MinimumLength = MerchantSettingConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        public virtual int MerchantId { get; set; }

        [StringLength(MerchantSettingConsts.MaxNsPayTitleLength, MinimumLength = MerchantSettingConsts.MinNsPayTitleLength)]
        public virtual string NsPayTitle { get; set; }

        [StringLength(MerchantSettingConsts.MaxLogoUrlLength, MinimumLength = MerchantSettingConsts.MinLogoUrlLength)]
        public virtual string LogoUrl { get; set; }

        [StringLength(MerchantSettingConsts.MaxLoginIpAddressLength, MinimumLength = MerchantSettingConsts.MinLoginIpAddressLength)]
        public virtual string LoginIpAddress { get; set; }

        [StringLength(MerchantSettingConsts.MaxBankNotifyLength, MinimumLength = MerchantSettingConsts.MinBankNotifyLength)]
        public virtual string BankNotify { get; set; }

        [StringLength(MerchantSettingConsts.MaxBankNotifyTextLength, MinimumLength = MerchantSettingConsts.MinBankNotifyTextLength)]
        public virtual string BankNotifyText { get; set; }

        [StringLength(MerchantSettingConsts.MaxTelegramNotifyBotIdLength, MinimumLength = MerchantSettingConsts.MinTelegramNotifyBotIdLength)]
        public virtual string TelegramNotifyBotId { get; set; }

        [StringLength(MerchantSettingConsts.MaxTelegramNotifyChatIdLength, MinimumLength = MerchantSettingConsts.MinTelegramNotifyChatIdLength)]
        public virtual string TelegramNotifyChatId { get; set; }

        public virtual bool OpenRiskWithdrawal { get; set; }

        [StringLength(MerchantSettingConsts.MaxPlatformUrlLength, MinimumLength = MerchantSettingConsts.MinPlatformUrlLength)]
        public virtual string PlatformUrl { get; set; }

        [StringLength(MerchantSettingConsts.MaxPlatformUserNameLength, MinimumLength = MerchantSettingConsts.MinPlatformUserNameLength)]
        public virtual string PlatformUserName { get; set; }

        [StringLength(MerchantSettingConsts.MaxPlatformPassWordLength, MinimumLength = MerchantSettingConsts.MinPlatformPassWordLength)]
        public virtual string PlatformPassWord { get; set; }

        public virtual decimal PlatformLimitMoney { get; set; }

        [StringLength(MerchantSettingConsts.MaxTelegramNotifyChatIdLength, MinimumLength = MerchantSettingConsts.MinTelegramNotifyChatIdLength)]
        public virtual string OrderBankRemark { get; set; }
    }
}
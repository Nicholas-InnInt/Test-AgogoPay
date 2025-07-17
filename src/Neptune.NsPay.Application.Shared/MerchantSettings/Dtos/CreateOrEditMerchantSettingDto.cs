using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantSettings.Dtos
{
    public class CreateOrEditMerchantSettingDto : EntityDto<int?>
    {

        [StringLength(MerchantSettingConsts.MaxMerchantCodeLength, MinimumLength = MerchantSettingConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

        public int MerchantId { get; set; }

        [StringLength(MerchantSettingConsts.MaxNsPayTitleLength, MinimumLength = MerchantSettingConsts.MinNsPayTitleLength)]
        public string NsPayTitle { get; set; }

        [StringLength(MerchantSettingConsts.MaxLogoUrlLength, MinimumLength = MerchantSettingConsts.MinLogoUrlLength)]
        public string LogoUrl { get; set; }

        [StringLength(MerchantSettingConsts.MaxLoginIpAddressLength, MinimumLength = MerchantSettingConsts.MinLoginIpAddressLength)]
        public string LoginIpAddress { get; set; }

        [StringLength(MerchantSettingConsts.MaxBankNotifyLength, MinimumLength = MerchantSettingConsts.MinBankNotifyLength)]
        public string BankNotify { get; set; }

        [StringLength(MerchantSettingConsts.MaxBankNotifyTextLength, MinimumLength = MerchantSettingConsts.MinBankNotifyTextLength)]
        public string BankNotifyText { get; set; }

        [StringLength(MerchantSettingConsts.MaxTelegramNotifyBotIdLength, MinimumLength = MerchantSettingConsts.MinTelegramNotifyBotIdLength)]
        public string TelegramNotifyBotId { get; set; }

        [StringLength(MerchantSettingConsts.MaxTelegramNotifyChatIdLength, MinimumLength = MerchantSettingConsts.MinTelegramNotifyChatIdLength)]
        public string TelegramNotifyChatId { get; set; }

        public bool OpenRiskWithdrawal { get; set; }

        [StringLength(MerchantSettingConsts.MaxPlatformUrlLength, MinimumLength = MerchantSettingConsts.MinPlatformUrlLength)]
        public string PlatformUrl { get; set; }

        [StringLength(MerchantSettingConsts.MaxPlatformUserNameLength, MinimumLength = MerchantSettingConsts.MinPlatformUserNameLength)]
        public string PlatformUserName { get; set; }

        [StringLength(MerchantSettingConsts.MaxPlatformPassWordLength, MinimumLength = MerchantSettingConsts.MinPlatformPassWordLength)]
        public string PlatformPassWord { get; set; }

        public decimal PlatformLimitMoney { get; set; }

    }
}
using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.MerchantSettings.Dtos
{
    public class GetAllMerchantSettingsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string MerchantCodeFilter { get; set; }

        public int? MaxMerchantIdFilter { get; set; }
        public int? MinMerchantIdFilter { get; set; }

        public string NsPayTitleFilter { get; set; }

        public string LogoUrlFilter { get; set; }

        public string LoginIpAddressFilter { get; set; }

        public string BankNotifyFilter { get; set; }

        public string BankNotifyTextFilter { get; set; }

        public string TelegramNotifyBotIdFilter { get; set; }

        public string TelegramNotifyChatIdFilter { get; set; }

        public int? OpenRiskWithdrawalFilter { get; set; }

        public string PlatformUrlFilter { get; set; }

        public string PlatformUserNameFilter { get; set; }

        public string PlatformPassWordFilter { get; set; }

        public decimal? MaxPlatformLimitMoneyFilter { get; set; }
        public decimal? MinPlatformLimitMoneyFilter { get; set; }

    }
}
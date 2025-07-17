using System;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.MerchantSettings.Dtos
{
    public class MerchantSettingDto : EntityDto
    {
        public string MerchantCode { get; set; }

        public int MerchantId { get; set; }

        public string NsPayTitle { get; set; }

        public string LogoUrl { get; set; }

        public string LoginIpAddress { get; set; }

        public string BankNotify { get; set; }

        public string BankNotifyText { get; set; }

        public string TelegramNotifyBotId { get; set; }

        public string TelegramNotifyChatId { get; set; }

        public bool OpenRiskWithdrawal { get; set; }

        public string PlatformUrl { get; set; }

        public string PlatformUserName { get; set; }

        public string PlatformPassWord { get; set; }

        public decimal PlatformLimitMoney { get; set; }

    }
}
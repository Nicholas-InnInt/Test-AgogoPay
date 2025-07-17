using Neptune.NsPay.MerchantConfig.Dto;
using Neptune.NsPay.MerchantWithdrawBanks.Dtos;
using System.Collections.Generic;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.MerchantConfig
{
    public class MerchantConfigViewModel
    {
        public virtual string MerchantCode { get; set; }

        public virtual int MerchantId { get; set; }

        public virtual bool AutoCloseBank { get; set; }

        public virtual bool ShowBankSort { get; set; }

        public virtual string Title { get; set; }

        public virtual string LogoUrl { get; set; }

        public virtual string LoginIpAddress { get; set; }

        public string BankNotifyText { get; set; }

        public string TelegramNotifyBotId { get; set; }

        public string TelegramNotifyChatId { get; set; }

        public List<MerchantConfigBank> MerchantConfigBank { get; set; }
        public bool OpenRiskWithdrawal { get; set; }
        public string PlatformUrl { get; set; }
        public string PlatformUserName { get; set; }
        public string PlatformPassWord { get; set; }
        public decimal PlatformLimitMoney { get; set; }
        public string OrderBankRemark { get; set; }

        public List<MerchantWithdrawBankDto> MerchantBanks { get; set; }
    }
}

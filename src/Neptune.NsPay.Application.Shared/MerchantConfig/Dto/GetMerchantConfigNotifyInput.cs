using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.MerchantConfig.Dto
{
    public class GetMerchantConfigNotifyInput
    {
        public string MerchantCode { get; set; }
        public int MerchantId { get; set; }
        public string BankNotifyText { get; set; }
        public string TelegramNotifyBotId { get; set; }

        public string TelegramNotifyChatId { get; set; }

        public List<MerchantConfigBank> MerchantConfigBank { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.MerchantConfig
{
    public class MerchantConfigConst
    {
        public const int MinMerchantTitleLength = 1;
        public const int MaxMerchantTitleLength = 50;
        public const string MerchantTitlePattern= "^[a-zA-Z][a-zA-Z0-9]*$";

        public const int MaxTelegramNotifyBotIdLength = 50;

        public const int MaxTelegramNotifyChatIdLength = 50;

        public const string BasicIpAddressRegex = @"^((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])(,((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9]))*$";
    }
}

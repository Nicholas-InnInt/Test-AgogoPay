using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.MerchantSettings.Dtos
{
    public class GetMerchantPlatFromWithdrawInput
    {
        public string MerchantCode { get; set; }
        public int MerchantId { get; set; }
        public bool OpenRiskWithdrawal { get; set; }
        public string PlatformUrl { get; set; }
        public string PlatformUserName { get; set; }
        public string PlatformPassWord { get; set; }
        public decimal PlatformLimitMoney { get; set; }
    }
}

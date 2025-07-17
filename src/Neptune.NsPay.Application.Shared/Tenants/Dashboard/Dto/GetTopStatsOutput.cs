using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.Tenants.Dashboard.Dto
{
    public class GetTopStatsOutput
    {
        public string TotalMerchantFund { get; set; }
        public int TotalMerchantCount { get; set; }
        public string TotalPayOrderFee { get; set; }
        public string TotalPayOrderMoney { get; set; }
        public int TotalPayOrderCount { get; set; }
        public string TotalMerchantWithdraw { get; set; }
        public int TotalMerchantWithdrawCount { get; set; }
        public string TotalTransferMoney { get; set; }
        public int TotalTransferCount { get; set; }
    }
}

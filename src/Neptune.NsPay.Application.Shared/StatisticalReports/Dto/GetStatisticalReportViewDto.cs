using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.StatisticalReports.Dto
{
    public class GetStatisticalReportViewDto
    {
        public int PayOrderSumCount { get; set; }
        public int PayOrderSuccessCount { get; set; }
        public string PayOrderSuccessBankMoney { get; set; }
        public string PayOrderSuccessScMoney { get; set; }
        public string payOrderBankFeeMoney { get; set; }
        public string PayOrderScFeeMoney { get; set; }
        public string PayOrderSuccessBankRate { get; set; }
        public string PayOrderSuccessScRate { get; set; }


        public int WithdrawOrderSumCount { get; set; }
        public int WithdrawOrderSuccessCount { get; set; }
        public string WithdrawOrderSuccessMoney { get; set; }
        public string WithdrawOrderFeeMoney { get; set; }
        public string WithdrawOrderSuccessRate { get; set; }


        public string MerchantSumBalance { get; set; }
        public string MerchantFrozenBalance { get; set; }
        
        public string MerchantWithdrawMoney { get; set; }
    }
}

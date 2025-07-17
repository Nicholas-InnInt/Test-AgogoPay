using Neptune.NsPay.PayMents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.BankBalance.Dto
{
    public class GetAllBankBalancesViewDto
    {
        public string PayName { get; set; }
        public string UserName { get; set; }
        public string MerchantName { get; set; }
        public PayMentTypeEnum PayType { get; set; }
        public DateTime LastTime { get; set; }
        public decimal Balance { get; set; }
        public string BalanceStr { get; set; }
        public bool Status { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.StatisticalBankReports.Dto
{
    public class GetOrderBankDetailViewDto
    {
        public string Name { get; set; }
        public string CardNumber { get; set; }
        public int OrderCount { get; set; }
        public string OrderMoney { get; set; }
        public int DepositcCRDTCount { get; set; }
        public int DepositcCRDTSuccessCount { get; set; }
        public string DepositcCRDTSuccessMoney { get; set; }
        public int DepositcCRDTAssociatedCount { get; set; }
        public string DepositcCRDTAssociatedMoney { get; set; }
        public int DepositcCRDTRejectCount { get; set; }
        public string DepositcCRDTRejectMoney { get; set; }
        public string DepositcDBITMoney { get; set; }
    }
}

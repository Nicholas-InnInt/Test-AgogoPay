using System;

namespace Neptune.NsPay.MerchantBills.Dtos
{
    public class GetMerchatBillForDashboardSummaryDto
    {
        public string Date { get; set; }
        public MerchantBillTypeEnum BillType { get; set; }
        public decimal TotalMoney { get; set; }

    }
}
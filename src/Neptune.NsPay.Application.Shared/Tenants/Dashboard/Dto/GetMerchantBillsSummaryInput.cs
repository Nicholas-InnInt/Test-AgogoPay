using System;

namespace Neptune.NsPay.Tenants.Dashboard.Dto
{
    public class GetMerchantBillsSummaryInput
    {
        public MerchantBillsSummaryDatePeriod MerchantBillsSummaryDatePeriod { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
    }
}
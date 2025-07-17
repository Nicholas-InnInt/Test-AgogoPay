using System.Collections.Generic;

namespace Neptune.NsPay.Tenants.Dashboard.Dto
{
    public class GetMerchantBillsSummaryOutput
    {
        public GetMerchantBillsSummaryOutput(List<MerchantBillsSummaryData> merchantBillsSummary)
        {
            MerchantBillsSummary = merchantBillsSummary;
        }

        //public int TotalSales { get; set; }

        //public decimal Revenue { get; set; }

        //public decimal Expenses { get; set; }


        public List<MerchantBillsSummaryData> MerchantBillsSummary { get; set; }

    }
}
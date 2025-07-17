namespace Neptune.NsPay.Tenants.Dashboard.Dto
{
    public class MerchantBillsSummaryData
    {
        public string Period { get; set; }
        public long OrderIn { get; set; }
        public long Withdraw { get; set; }

        public MerchantBillsSummaryData(string period, long orderIn, long withdraw)
        {
            Period = period;
            OrderIn = orderIn;
            Withdraw = withdraw;
        }
    }
}
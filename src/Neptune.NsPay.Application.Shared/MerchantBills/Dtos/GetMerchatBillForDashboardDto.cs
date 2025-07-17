namespace Neptune.NsPay.MerchantBills.Dtos
{
    public class GetMerchatBillForDashboardDto 
    {
        public MerchantBillTypeEnum BillType { get; set; }
        public int TotalCount { get; set; }
        public decimal TotalMoney { get; set; }
        public decimal TotalFeeMoney { get; set; }

    }
}
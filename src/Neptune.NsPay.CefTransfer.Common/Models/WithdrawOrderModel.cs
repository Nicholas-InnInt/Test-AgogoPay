namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class WithdrawOrderModel
    {
        public string Id { get; set; }
        public string MerchantCode { get; set; }
        public string OrderNo { get; set; }
        public string BenBankName { get; set; }
        public string BenAccountNo { get; set; }
        public string BenAccountName { get; set; }
        public decimal OrderMoney { get; set; }
        public int DeviceId { get; set; }
    }
}

namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class GetWithdrawOrderModel
    {
        public string MerchantCode { get; set; }
        public string Phone { get; set; }
        public int BankType { get; set; }
    }
}

namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class UpdateWithdrawOrderModel
    {
        public string OrderId { get; set; }
        public string TransactionNo { get; set; }
        public int OrderStatus { get; set; }
        public string Remark { get; set; }
    }
}
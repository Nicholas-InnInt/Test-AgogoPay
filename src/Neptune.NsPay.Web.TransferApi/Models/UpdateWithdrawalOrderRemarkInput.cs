using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class UpdateWithdrawalOrderRemarkInput
    {
        public string OrderId { get; set; }
        public string? Remark { get; set; }
    }
}

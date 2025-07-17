using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class UpdateWithdrawOrderInput
    {
        public string OrderId { get; set; }
        public string TransactionNo { get; set; }

        public int? DeviceId { get; set; }
        public WithdrawalOrderStatusEnum OrderStatus { get; set; }
        public string? Remark { get; set; }
    }
}

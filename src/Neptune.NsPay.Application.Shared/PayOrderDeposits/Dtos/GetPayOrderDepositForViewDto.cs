using Neptune.NsPay.PayOrders.Dtos;

namespace Neptune.NsPay.PayOrderDeposits.Dtos
{
    public class GetPayOrderDepositForViewDto
    {
        public PayOrderDepositDto PayOrderDeposit { get; set; }
        public PayOrderDto PayOrder { get; set; }
        public int Type { get; set; }
    }
}
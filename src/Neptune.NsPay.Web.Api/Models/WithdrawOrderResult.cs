namespace Neptune.NsPay.Web.Api.Models
{
    public class WithdrawOrderResult
    {
        public string OrderNo { get; set; }
        public string TradeNo { get; set; }
        public decimal Money { get; set; }
    }
}

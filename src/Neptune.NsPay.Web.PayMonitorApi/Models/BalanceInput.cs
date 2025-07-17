namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class BalanceInput : BaseInput
    {
        public int PayMentId { get; set; }
        public decimal Balance { get; set; }

    }
}

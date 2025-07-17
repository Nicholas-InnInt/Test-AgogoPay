namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class BalanceUpdateNotification
    {
        public int PayMentId { get; set; }
        public decimal Balance { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}

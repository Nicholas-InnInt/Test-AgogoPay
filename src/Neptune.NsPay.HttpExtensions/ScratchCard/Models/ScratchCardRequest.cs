namespace Neptune.NsPay.HttpExtensions.ScratchCard.Models
{
    public class ScratchCardRequest
    {
        public string OrderId { get; set; }
        public int PayMentId { get; set; }
        public string TelcoName { get; set; }
        public decimal Amount { get; set; }
        public string Seri { get; set; }
        public string Code { get; set; }
        public string Transactionid { get; set; }
        public string CallUrl { get; set; }
    }
}

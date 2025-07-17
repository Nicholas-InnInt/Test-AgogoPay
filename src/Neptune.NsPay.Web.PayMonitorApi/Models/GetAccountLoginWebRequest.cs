namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class GetAccountLoginWebRequest
    {
        public int PayMentId { get; set; } // PaymentId
        public string? Sign { get; set; } // Account Phone Number
        public string? MerchantCode { get; set; } // Account Bank
    }
}

namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class PaySessionRequest
    {
        public string? Account { get; set; }
        public int PayMentId { get; set; }
        public string? Sign { get; set; }
        public string? MerchantCode { get; set; }
        public string? Phone { get; set; }
        public string? PayType { get; set; }
        public string? SessionId { get; set; }
        public string? Code { get; set; }
        public string? CaptchaId { get; set; }
        public string? TechcomCookie { get; set; }
        public string? BIDVSessionData { get; set; } //  json content of PaySessionBidvModel
        public int Status { get; set; }
        public decimal Balance { get; set; }
        public string? Token { get; set; }
        public string? Cookies { get; set; }
    }
}

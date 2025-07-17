namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class SetAccountLoginRequest
    {
        public string? Account { get; set; } // Bankcard Number
        public int PayMentId { get; set; } // PaymentId
        public string? Sign { get; set; } 
        public string? Phone { get; set; } // Account Phone Number
        public string? MerchantCode { get; set; } // Merchant Code
        public string? LoginOTP { get; set; } // LoginOTP

    }
}

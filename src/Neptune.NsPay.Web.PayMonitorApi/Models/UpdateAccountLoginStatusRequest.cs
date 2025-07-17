namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class UpdateAccountLoginStatusRequest
    {
        public string? Account { get; set; } // Bankcard Number
        public string? TypeStr { get; set; } // Account Phone Number
        public string? Phone { get; set; } // Account Bank
        public string? MerchantCode { get; set; } // Merchant Code
        public string? OTP { get; set; } 
        public int Status { get; set; } 

    }
}

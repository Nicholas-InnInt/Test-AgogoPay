namespace Neptune.NsPay.BankApi.Models
{
    public class BankJobInputRequest
    {
        public string? MerchantCode { get; set; }
        public string Phone { get; set; }
        public string BankApi { get; set; }

        public string? CardNumber { get; set; }

        public  int Debbug { get; set; }
    }
}

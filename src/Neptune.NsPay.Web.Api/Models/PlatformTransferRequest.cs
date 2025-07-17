namespace Neptune.NsPay.Web.Api.Models
{
    public class PlatformTransferRequest
    {
        public string MerchNo { get; set; }
        public string OrderNo { get; set; }
        public decimal Money { get; set; }
        public string BankAccNo { get; set; }
        public string BankAccName { get; set; }
        public string BankName { get; set; }
        public string Desc { get; set; }
        //public string UserNo { get; set; }
        public string NotifyUrl { get; set; }
        public string Sign { get; set; }
    }
}

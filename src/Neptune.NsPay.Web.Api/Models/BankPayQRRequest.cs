namespace Neptune.NsPay.Web.Api.Models
{
    public class BankPayQRRequest
    {
        public string OrderId { get; set; }
        public string BankCode { get; set; }
        public int PayId { get; set; }
    }
}

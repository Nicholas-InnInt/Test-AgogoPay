namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class GetOrderOtpInput
    {
        public int DeviceId { get; set; }
        public string? OrderId { get; set; }
    }
}

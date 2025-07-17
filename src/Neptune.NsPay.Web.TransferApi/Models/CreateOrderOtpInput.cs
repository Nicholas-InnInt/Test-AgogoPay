namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class CreateOrderOtpInput
    {
        public int DeviceId { get; set; }
        public string? OrderId { get; set; }
        public string TransferOtp { get; set; }
    }
}

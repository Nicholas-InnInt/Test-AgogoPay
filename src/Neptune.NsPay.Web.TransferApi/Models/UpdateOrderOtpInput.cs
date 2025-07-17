namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class UpdateOrderOtpInput
    {
        public int DeviceId { get; set; }
        public string? OrderId { get; set; }
        public string Otp { get; set; }
    }
}

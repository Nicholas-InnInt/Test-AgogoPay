namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class CreateOrderOtpModel
    {
        public int DeviceId { get; set; }
        public string OrderId { get; set; }
        public string TransferOtp { get; set; }
    }
}

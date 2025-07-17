using Neptune.NsPay.WithdrawalDevices;

namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class OrderOtpModel
    {
        public int DeviceId { get; set; }
        public string OrderId { get; set; }
        public string Phone { get; set; }
        public WithdrawalDevicesBankTypeEnum BankType { get; set; }
        public string Otp { get; set; }
        public int OrderStatus { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
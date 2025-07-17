using Neptune.NsPay.WithdrawalDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class TransferOrderOtpModel
    {
        public int DeviceId { get; set; }
        public string OrderId { get; set; }
        public string Phone { get; set; }
        public WithdrawalDevicesBankTypeEnum BankType { get; set; }
        public string TransferOtp { get; set; }
        public string Otp { get; set; }
        public int OrderStatus { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}

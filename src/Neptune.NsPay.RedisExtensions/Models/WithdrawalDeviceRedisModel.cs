using Neptune.NsPay.WithdrawalDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class WithdrawalDeviceRedisModel
    {
        public int Id { get; set; }
        public string MerchantCode { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public WithdrawalDevicesBankTypeEnum BankType { get; set; }
        public bool Status { get; set; }
        public string CardName { get; set; }
        public WithdrawalDevicesProcessTypeEnum Process { get; set; }
        public string LoginPassWord { get; set; }
        public string BankOtp { get; set; }
        public string DeviceAdbName { get; set; }

        public decimal MinMoney { get; set; }

        public decimal MaxMoney { get; set; }

        public decimal Balance { get; set; }
    }
}

using Neptune.NsPay.WithdrawalDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class WithdrawBalanceModel
    {
        public int DeviceId { get; set; }
        public string Phone { get; set; }
        public WithdrawalDevicesBankTypeEnum BankType { get; set; }
        public decimal Balance { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}

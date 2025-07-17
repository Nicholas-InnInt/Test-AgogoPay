using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class WithdrawalDeviceResultDto
    {
        public List<string> SuccessOrder { get; set; }
        public List<string> FailedOrder { get; set; }
        public bool IsSuccess { get; set; }

    }
}

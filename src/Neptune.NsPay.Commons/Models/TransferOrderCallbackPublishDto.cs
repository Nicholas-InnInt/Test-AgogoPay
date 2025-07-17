using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons.Models
{
    public class TransferOrderCallbackPublishDto
    {
        public string WithdrawalOrderId { get; set; }
        public string ProcessId { get; set; } // for Reference Id
    }
}
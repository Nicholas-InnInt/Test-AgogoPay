using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons.Models
{
    public class PayOrderCallbackPublishDto
    {
        public string PayOrderId { get; set; }
        public string ProcessId { get; set; } // for Reference Id
    }
}

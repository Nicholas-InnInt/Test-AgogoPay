using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class LogUpdateModel
    {
        public string Topic { get; set; }
        public string Message { get; set; }
        public DateTime @DateTime { get; set; }
        public string Photo { get; set; }
        public string PaymentId { get; set; }
        public string Phone { get; set; }
    }
}

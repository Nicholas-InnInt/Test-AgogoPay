using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class AppLogUpdateModel
    {
        public List<string> Messages { get; set; } = new List<string>();
        public string Html { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}

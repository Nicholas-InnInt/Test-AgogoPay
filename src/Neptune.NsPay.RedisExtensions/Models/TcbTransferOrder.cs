using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class TcbTransferOrder
    {
        public int DeviceId { get; set; }
        public string OrderId { get; set; }
        public string Phone { get; set; }

        public int Status { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}

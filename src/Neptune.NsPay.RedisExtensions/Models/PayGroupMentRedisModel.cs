using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class PayGroupMentRedisModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string BankApi { get; set; }
        public string VietcomApi { get; set; }
        public bool Status { get; set; }

        public List<PayMentRedisModel> PayMents { get; set; }
    }
}

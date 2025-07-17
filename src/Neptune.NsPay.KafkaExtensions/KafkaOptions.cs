using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.KafkaExtensions
{
    public class KafkaOptions
    {
        public string BootstrapServers { get; set; }
        public List<string> Topics { get; set; }
        public string GroupId { get; set; }
        public string SaslUsername { get; set; }
        public string SaslPassword { get; set; }
    }
}

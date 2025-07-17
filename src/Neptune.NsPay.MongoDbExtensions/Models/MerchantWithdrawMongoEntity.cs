using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
    public class MerchantWithdrawMongoEntity
    {
        public string MerchantCode { get; set; }
        public int MerchantId { get; set; }
        public string WithDrawNo { get; set; }
        public decimal Money { get; set; }
        public DateTime ReviewTime { get; set; }

        public DateTime CreationTime { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons.Models
{
    public class LockedBalanceDto
    {

        public string MerchantCode
        {
            get; set;
        }
        public string WithdrawalId
        {
            get; set;
        }

        public string ReferenceEventId
        {
            get; set;
        }
    }
}

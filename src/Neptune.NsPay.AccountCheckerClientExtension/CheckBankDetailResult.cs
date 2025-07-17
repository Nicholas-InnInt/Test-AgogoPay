using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.AccountCheckerClientExtension
{
    public class CheckBankDetailResult
    {
        public string bankName { get; set; }
        public string accountNumber { get; set; }

        public string holderName { get; set; }

        public int? paymentId { get; set; } // account which use to get holder name 

        public bool? success { get; set; } // true if call api success 

        public string errorMessage { get; set; }    // message on failed reason

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.AccountNameChecker.Dto
{
    public class CheckBankDetailResultDto
    {

            public string bankName { get; set; }
            public string accountNumber { get; set; }

            public string holderName { get; set; }

            public int? paymentId { get; set; } // account which use to get holder name 

            public bool? success { get; set; } // true if call api success 

            public string errorMessage { get; set; }    // message on failed reason

    }
}

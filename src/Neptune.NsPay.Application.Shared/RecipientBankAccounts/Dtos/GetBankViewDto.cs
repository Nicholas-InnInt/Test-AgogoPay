using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.RecipientBankAccounts.Dtos
{
    public class GetBankViewDto
    {
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string BankShortName { get; set; }
    }
}

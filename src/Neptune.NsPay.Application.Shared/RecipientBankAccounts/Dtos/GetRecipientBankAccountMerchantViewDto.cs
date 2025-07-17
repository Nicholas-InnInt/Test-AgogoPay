using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.RecipientBankAccounts.Dtos
{
    public class GetRecipientBankAccountMerchantViewDto
    {
        public int MerchantId { get; set; }
        public string MerchantCode { get; set; }
        public string MerchantName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.RecipientBankAccounts.Dtos
{
    public class ImportRecipientBankAccountExcelDto
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
    }

    public class ImportRecipientBankExcelContentDto
    {
        public string MerchantCode { get; set; }

        public string BankName { get; set; }
        public string HolderName { get; set; }

        public string AccountNumber { get; set; }

    }
}

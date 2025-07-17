using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.RecipientBankAccounts.Dtos
{
    public class ImportResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Duplicates { get; set; }
    }

}

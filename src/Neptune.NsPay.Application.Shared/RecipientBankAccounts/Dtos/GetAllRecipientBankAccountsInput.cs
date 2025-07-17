using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.RecipientBankAccounts.Dtos
{
    public class GetAllRecipientBankAccountsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string HolderName { get; set; }

        public string AccountNumber { get; set; }

        public string BankCode { get; set; }
    }
}

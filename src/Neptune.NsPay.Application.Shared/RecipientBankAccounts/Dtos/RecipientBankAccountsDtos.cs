using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.RecipientBankAccounts.Dtos
{
    public class RecipientBankAccountsDtos : EntityDto<string>
    {
        public string Id { get; set; }
        public int? MerchantId { get; set; } 
        public string HolderName { get; set; }
        public string AccountNumber { get; set; }
        public string BankCode { get; set; }
        public string BankKey { get; set; }
        public int? VerifyDeviceId { get; set; } 
        public int? VerifyPaymentId { get; set; }
        public string CreatedBy { get; set; }
        public string MerchantCode { get; set; }
    }
}

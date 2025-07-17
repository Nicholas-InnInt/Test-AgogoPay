using System;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.MerchantWithdrawBanks.Dtos
{
    public class MerchantWithdrawBankDto : EntityDto
    {
        public string MerchantCode { get; set; }

        public string BankName { get; set; }

        public string ReceivCard { get; set; }

        public string ReceivName { get; set; }

        public bool Status { get; set; }

    }
}
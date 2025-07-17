using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantWithdrawBanks.Dtos
{
    public class GetMerchantWithdrawBankForEditOutput
    {
        public CreateOrEditMerchantWithdrawBankDto MerchantWithdrawBank { get; set; }

    }
}
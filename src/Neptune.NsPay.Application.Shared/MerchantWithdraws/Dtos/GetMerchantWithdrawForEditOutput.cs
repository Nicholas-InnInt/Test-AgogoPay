using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;
using Neptune.NsPay.MerchantWithdrawBanks.Dtos;
using System.Collections.Generic;

namespace Neptune.NsPay.MerchantWithdraws.Dtos
{
    public class GetMerchantWithdrawForEditOutput
    {
        public CreateOrEditMerchantWithdrawDto MerchantWithdraw { get; set; }
        public List<MerchantWithdrawBankDto> MerchantBanks { get; set; }        
        public decimal? Balance { get; set; }
        public decimal PendingWithdrawalOrderAmount { get; set; }

        public decimal PendingMerchantWithdrawalAmount { get; set; }

        public decimal? BalanceInit { get; set; }

    }
}
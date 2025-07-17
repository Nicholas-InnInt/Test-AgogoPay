using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantFunds.Dtos
{
    public class CreateOrEditMerchantFundDto : EntityDto<int?>
    {

        [StringLength(MerchantFundConsts.MaxMerchantCodeLength, MinimumLength = MerchantFundConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

        public int MerchantId { get; set; }

        public decimal DepositAmount { get; set; }

        public decimal WithdrawalAmount { get; set; }

        public decimal RateFeeBalance { get; set; }

        public decimal Balance { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime UpdateTime { get; set; }

    }
}
using System;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.MerchantFunds.Dtos
{
    public class MerchantFundDto : EntityDto
    {
        public string MerchantCode { get; set; }

        public int MerchantId { get; set; }

        public decimal DepositAmount { get; set; }

        public decimal WithdrawalAmount { get; set; }

        public decimal TranferAmount { get; set; }

        public decimal RateFeeBalance { get; set; }

        public decimal Balance { get; set; }

        public decimal FrozenBalance { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime UpdateTime { get; set; }

    }
}
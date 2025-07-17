using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.MerchantFunds.Dtos
{
    public class GetAllMerchantFundsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string MerchantCodeFilter { get; set; }

        public int? MaxMerchantIdFilter { get; set; }
        public int? MinMerchantIdFilter { get; set; }

        public decimal? MaxDepositAmountFilter { get; set; }
        public decimal? MinDepositAmountFilter { get; set; }

        public decimal? MaxWithdrawalAmountFilter { get; set; }
        public decimal? MinWithdrawalAmountFilter { get; set; }

        public decimal? MaxRateFeeBalanceFilter { get; set; }
        public decimal? MinRateFeeBalanceFilter { get; set; }

        public decimal? MaxBalanceFilter { get; set; }
        public decimal? MinBalanceFilter { get; set; }

        public DateTime? MaxCreationTimeFilter { get; set; }
        public DateTime? MinCreationTimeFilter { get; set; }

        public DateTime? MaxUpdateTimeFilter { get; set; }
        public DateTime? MinUpdateTimeFilter { get; set; }

    }
}
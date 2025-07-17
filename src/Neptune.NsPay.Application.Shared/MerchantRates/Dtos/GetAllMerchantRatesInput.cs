using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.MerchantRates.Dtos
{
    public class GetAllMerchantRatesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string MerchantCodeFilter { get; set; }

        public int? MaxMerchantIdFilter { get; set; }
        public int? MinMerchantIdFilter { get; set; }

        public decimal? MaxScanBankRateFilter { get; set; }
        public decimal? MinScanBankRateFilter { get; set; }

        public decimal? MaxScratchCardRateFilter { get; set; }
        public decimal? MinScratchCardRateFilter { get; set; }

        public decimal? MaxMoMoRateFilter { get; set; }
        public decimal? MinMoMoRateFilter { get; set; }

    }
}
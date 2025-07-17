using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.Merchants.Dtos
{
    public class GetAllMerchantsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string NameFilter { get; set; }

        public string MerchantCodeFilter { get; set; }

        public string PlatformCodeFilter { get; set; }

    }
}
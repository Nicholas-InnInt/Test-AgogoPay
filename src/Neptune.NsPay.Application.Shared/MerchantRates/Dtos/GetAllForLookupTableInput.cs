﻿using Abp.Application.Services.Dto;

namespace Neptune.NsPay.MerchantRates.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
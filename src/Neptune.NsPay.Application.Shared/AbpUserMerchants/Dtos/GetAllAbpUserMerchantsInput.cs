using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.AbpUserMerchants.Dtos
{
    public class GetAllAbpUserMerchantsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

    }
}
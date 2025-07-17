using Abp.Application.Services.Dto;

namespace Neptune.NsPay.AbpUserMerchants.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
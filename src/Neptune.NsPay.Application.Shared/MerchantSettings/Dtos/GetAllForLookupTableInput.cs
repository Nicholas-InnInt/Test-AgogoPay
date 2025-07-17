using Abp.Application.Services.Dto;

namespace Neptune.NsPay.MerchantSettings.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
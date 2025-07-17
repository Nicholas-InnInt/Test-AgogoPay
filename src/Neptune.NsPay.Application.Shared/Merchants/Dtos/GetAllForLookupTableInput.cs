using Abp.Application.Services.Dto;

namespace Neptune.NsPay.Merchants.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
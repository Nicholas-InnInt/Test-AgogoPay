using Abp.Application.Services.Dto;

namespace Neptune.NsPay.PayGroups.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
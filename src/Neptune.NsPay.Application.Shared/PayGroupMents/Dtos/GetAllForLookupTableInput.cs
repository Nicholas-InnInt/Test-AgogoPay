using Abp.Application.Services.Dto;

namespace Neptune.NsPay.PayGroupMents.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
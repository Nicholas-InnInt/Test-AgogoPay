using Abp.Application.Services.Dto;

namespace Neptune.NsPay.PayMents.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
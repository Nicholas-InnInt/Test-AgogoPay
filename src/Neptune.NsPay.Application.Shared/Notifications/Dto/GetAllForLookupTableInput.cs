using Abp.Application.Services.Dto;

namespace Neptune.NsPay.Notifications.Dto
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.NsPayBackgroundJobs.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
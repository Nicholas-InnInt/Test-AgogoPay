using Abp.Application.Services.Dto;

namespace Neptune.NsPay.PayOrders.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
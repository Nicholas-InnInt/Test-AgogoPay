using Abp.Application.Services.Dto;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
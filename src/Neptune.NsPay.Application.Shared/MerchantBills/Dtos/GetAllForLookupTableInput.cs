using Abp.Application.Services.Dto;

namespace Neptune.NsPay.MerchantBills.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
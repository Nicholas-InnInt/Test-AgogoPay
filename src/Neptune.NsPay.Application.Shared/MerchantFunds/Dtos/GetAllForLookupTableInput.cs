using Abp.Application.Services.Dto;

namespace Neptune.NsPay.MerchantFunds.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
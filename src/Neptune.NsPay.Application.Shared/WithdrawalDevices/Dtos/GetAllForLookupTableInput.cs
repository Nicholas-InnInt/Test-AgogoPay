using Abp.Application.Services.Dto;

namespace Neptune.NsPay.WithdrawalDevices.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
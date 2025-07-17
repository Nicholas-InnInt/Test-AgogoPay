using Abp.Application.Services.Dto;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class WithdrawalOrderPageResultDto<T> : PagedResultDto<T>
    {
        public decimal OrderMoneyTotal { get; set; }
        public decimal FeeMoneyTotal { get; set; }
    }
}
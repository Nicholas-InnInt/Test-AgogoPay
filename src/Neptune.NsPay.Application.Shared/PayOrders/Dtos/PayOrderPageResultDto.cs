using Abp.Application.Services.Dto;

namespace Neptune.NsPay.PayOrders.Dtos
{
    public class PayOrderPageResultDto<T> : PagedResultDto<T>
    {
        public decimal OrderMoneyTotal { get; set; }
        public decimal FeeMoneyTotal { get; set; }
    }
}
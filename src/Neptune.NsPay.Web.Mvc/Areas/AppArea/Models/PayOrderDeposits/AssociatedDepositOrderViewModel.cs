using Abp.Extensions;
using Neptune.NsPay.PayOrders.Dtos;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.PayOrderDeposits
{
    public class AssociatedDepositOrderViewModel
    {
        public AssociatedDepositOrderCallBackDto AssociatedOrder { get; set; }
        public bool IsEditMode => !AssociatedOrder.Id.IsNullOrEmpty();
    }
}

using Neptune.NsPay.PayOrders.Dtos;

using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.PayOrders
{
    public class CreateOrEditPayOrderModalViewModel
    {
        public CreateOrEditPayOrderDto PayOrder { get; set; }

        public bool IsEditMode => !PayOrder.Id.IsNullOrEmpty();
    }
}
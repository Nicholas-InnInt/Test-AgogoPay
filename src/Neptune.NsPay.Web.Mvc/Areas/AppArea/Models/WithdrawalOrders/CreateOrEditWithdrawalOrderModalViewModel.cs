using Abp.Collections.Extensions;
using Neptune.NsPay.WithdrawalOrders.Dtos;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.WithdrawalOrders
{
    public class CreateOrEditWithdrawalOrderModalViewModel
    {
        public CreateOrEditWithdrawalOrderDto WithdrawalOrder { get; set; }

        public bool IsEditMode => !WithdrawalOrder.Id.IsNullOrEmpty();
    }
}
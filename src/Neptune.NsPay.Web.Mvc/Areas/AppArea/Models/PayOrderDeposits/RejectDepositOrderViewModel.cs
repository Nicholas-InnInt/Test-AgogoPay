

using Abp.Extensions;
using Neptune.NsPay.PayOrders.Dtos;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.PayOrderDeposits
{
    public class RejectDepositOrderViewModel 
    {
        public RejectPayOrderDepositDto RejectOrder { get; set; }
        public bool IsEditMode => !RejectOrder.Id.IsNullOrEmpty() ;
    }
}
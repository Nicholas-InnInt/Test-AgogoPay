using System.Collections.Generic;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.PayOrders
{
    public class PayOrderModel
    {
        public List<EditPayOrder> ListEditPayOrder { get; set; }

        public List<EditPayOrder> ListOfPayOrderNumber { get; set; }

        public PayOrderModel()
        {
            ListEditPayOrder = new List<EditPayOrder>();
        }

        public int OrderTotalCount { get; set; }

    }


    public class EditPayOrder
    {
        public string OrderId { get; set; }
        public string OrderNumber { get; set; }

    }
}

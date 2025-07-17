using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.PayOrders.Dtos
{
    public class PayOrderBatchNotificationInput
    {

        public string PayOrderID { get; set; }
        public string PayOrderNumber  { get; set; }
    }
}

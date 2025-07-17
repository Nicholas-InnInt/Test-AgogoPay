using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.PayOrders
{
    public enum PayOrderOrderStatusEnum
    {
        NotPaid = 1,//等待支付
        Paid = 2,//支付中
        Failed = 3,//支付失败
        TimeOut = 4,//订单超时
        Completed = 5//支付完成
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.PayOrders
{
    public enum PayOrderOrderTypeEnum
    {
        Receive = 1,//入款
        Enforce = 2,//强制补单
        Justify = 3,//补单
        TopUp = 4,//充值余额
    }
}

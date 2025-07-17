using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.PayOrders
{
    public enum PayOrderScoreStatusEnum
    {
        NoScore = 0,
        Completed = 1,//上分成功
        Failed = 2,//上分失败
    }
}

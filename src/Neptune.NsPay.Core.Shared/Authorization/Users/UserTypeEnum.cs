using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.Authorization.Users
{
    public enum UserTypeEnum
    {
        InternalMerchant = 1,
        ExternalMerchant = 2,
        NsPayAdmin = 3,
        NsPayKefu = 4,
    }
}

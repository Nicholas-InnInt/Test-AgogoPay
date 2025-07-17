using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.Authorization.Users.Dto
{
    public class UserMerchantDto
    {
        public int MerchantId { get; set; }

        public string MerchantName { get; set; }

        public string MerchantDisplayName { get; set; }

        public bool IsAssigned { get; set; }
    }
}

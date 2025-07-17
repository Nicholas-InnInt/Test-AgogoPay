using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.MerchantConfig.Dto
{
    public class GetMerchantConfigIpAddressInput
    {
        public string MerchantCode { get; set; }
        public int MerchantId { get; set; }
        public string LoginIpAddress { get; set; }
    }
}

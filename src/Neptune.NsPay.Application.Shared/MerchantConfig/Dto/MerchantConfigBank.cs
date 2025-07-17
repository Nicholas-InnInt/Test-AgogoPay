using Neptune.NsPay.PayMents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.MerchantConfig.Dto
{
    public class MerchantConfigBank
    {
        public PayMentTypeEnum Type { get; set; }
        public bool IsOpen { get; set; }
        public decimal Money { get; set; }
    }
}

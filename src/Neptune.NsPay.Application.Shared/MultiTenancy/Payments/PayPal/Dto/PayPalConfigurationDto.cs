﻿using System.Collections.Generic;

namespace Neptune.NsPay.MultiTenancy.Payments.PayPal.Dto
{
    public class PayPalConfigurationDto
    {
        public string ClientId { get; set; }

        public string DemoUsername { get; set; }

        public string DemoPassword { get; set; }
        
        public List<string> DisabledFundings { get; set; }
    }
}

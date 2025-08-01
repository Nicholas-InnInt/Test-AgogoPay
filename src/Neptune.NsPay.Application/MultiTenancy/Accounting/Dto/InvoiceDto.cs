﻿using System;
using System.Collections.Generic;
using Neptune.NsPay.MultiTenancy.Payments.Dto;
using Z.EntityFramework.Extensions;

namespace Neptune.NsPay.MultiTenancy.Accounting.Dto
{
    public class InvoiceDto
    {
        public List<SubscriptionPaymentProductDto> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }
        
        public string InvoiceNo { get; set; }

        public DateTime InvoiceDate { get; set; }

        public string TenantLegalName { get; set; }

        public List<string> TenantAddress { get; set; }

        public string TenantTaxNo { get; set; }

        public string HostLegalName { get; set; }

        public List<string> HostAddress { get; set; } = new();
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class GetDisplayProofDto
    {
        public string Base64Content { get; set; }

        public string OrderNumber { get; set; }
    }
}

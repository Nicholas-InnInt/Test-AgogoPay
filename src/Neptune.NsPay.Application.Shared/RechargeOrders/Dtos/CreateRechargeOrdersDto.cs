using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.RechargeOrders.Dtos
{
    public class CreateRechargeOrdersDto
    {
        public string MerchantCode { get; set; }
        public decimal OrderMoney { get; set; }
        public int ComputeRate { get; set; }
        public string Remark { get; set; }
    }
}

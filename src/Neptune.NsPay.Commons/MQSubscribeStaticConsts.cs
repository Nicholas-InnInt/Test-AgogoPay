using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.Commons
{
    public class MQSubscribeStaticConsts
    {
        //代收订单
        public const string MerchantBillAddBalance = "NsPay.MerchantBillService.AddBalance";

        //商户提现
        public const string MerchantBillAddWithdraws = "NsPay.MerchantBillService.AddWithdraws";

        //代付订单
        public const string MerchantBillReduceBalance = "NsPay.MerchantBillService.ReduceBalance";

        public const string BankOrderNotify = "NsPay.BankOrderNotify";
    }
}

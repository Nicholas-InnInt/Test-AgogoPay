using Abp;
using Neptune.NsPay.PayOrderDeposits.Dtos;

namespace Neptune.NsPay.PayOrders.Exporting
{
    public class PayOrderDepositsExcelJobArgs
    {
        public GetAllPayOrderDepositsForExcelInput input { get; set; }
        public UserIdentifier User { get; set; }
    }
}

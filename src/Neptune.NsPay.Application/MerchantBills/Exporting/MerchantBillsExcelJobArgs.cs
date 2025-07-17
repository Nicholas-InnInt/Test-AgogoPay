using Abp;
using Neptune.NsPay.MerchantBills.Dtos;

namespace Neptune.NsPay.PayOrders.Exporting
{
    public class MerchantBillsExcelJobArgs
    {
        public GetAllMerchantBillsForExcelInput input { get; set; }
        public UserIdentifier User { get; set; }
    }
}

using Abp;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.PayOrders.Dtos;

namespace Neptune.NsPay.PayOrders.Exporting
{
    public class PayOrderExcelJobArgs
    {
        public GetAllPayOrdersForExcelInput input { get; set; }
        public UserIdentifier User { get; set; }
        public UserTypeEnum UserType { get; set; }
    }
}

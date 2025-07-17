using Abp;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.WithdrawalOrders.Dtos;

namespace Neptune.NsPay.WithdrawalOrders.Exporting
{
    public class WithdrawalOrderExcelJobArgs
    {
        public GetAllWithdrawalOrdersForExcelInput input { get; set; }
        public UserIdentifier User { get; set; }
        public UserTypeEnum UserType { get; set; }
    }
}

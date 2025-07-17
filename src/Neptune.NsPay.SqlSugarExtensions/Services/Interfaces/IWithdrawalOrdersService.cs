using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.WithdrawalOrders;

namespace Neptune.NsPay.SqlSugarExtensions.Services.Interfaces
{
    public interface IWithdrawalOrdersService : IBaseService<WithdrawalOrder>
    {
        Task<bool> UpsertWithdrawalOrders(List<WithdrawalOrder> datalist);
    }
}

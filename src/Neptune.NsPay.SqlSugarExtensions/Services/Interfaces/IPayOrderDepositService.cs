using Neptune.NsPay.PayOrderDeposits;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;

namespace Neptune.NsPay.SqlSugarExtensions.Services.Interfaces
{
    public interface IPayOrderDepositService : IBaseService<PayOrderDeposit>
    {
        Task<bool> UpsertPayOrderDeposit(List<PayOrderDeposit> datalist);
    }
}

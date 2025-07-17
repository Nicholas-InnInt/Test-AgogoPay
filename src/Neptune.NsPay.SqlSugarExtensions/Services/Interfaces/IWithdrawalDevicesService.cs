using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.WithdrawalDevices;

namespace Neptune.NsPay.SqlSugarExtensions.Services.Interfaces
{
    public interface IWithdrawalDevicesService : IBaseService<WithdrawalDevice>
    {
        Task<List<WithdrawalDevice>> GetWithdrawDevices(string merchantCode);
    }
}
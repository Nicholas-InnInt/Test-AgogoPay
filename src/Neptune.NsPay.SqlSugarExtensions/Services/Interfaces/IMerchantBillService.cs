
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;

namespace Neptune.NsPay.SqlSugarExtensions.Services.Interfaces
{
    public interface IMerchantBillService : IBaseService<MerchantBill>
    {
        Task<bool> UpsertMerchantBills(List<MerchantBill> datalist);
    }
}

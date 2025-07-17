using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using SqlSugar;
using NLog.Filters;
using Abp.Json;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class MerchantBillService : BaseService<MerchantBill>, IMerchantBillService, IDisposable
    {
        private static readonly object Locker = new object();

        public MerchantBillService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<bool> UpsertMerchantBills(List<MerchantBill> datalist) 
        {
            try
            {
                var sp = "Upsert_MerchantBills";
                var P_dataList = new SugarParameter("@dataList", datalist.ToJsonString());

                await Db.Ado.UseStoredProcedure().ExecuteCommandAsync(sp, P_dataList);
            }
            catch (Exception ex)
            {
                return false;
            }
            
            return true;
        }

        public void Dispose()
        {
        }
    }
}

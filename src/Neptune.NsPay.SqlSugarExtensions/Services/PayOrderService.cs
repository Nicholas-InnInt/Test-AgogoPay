using Abp.Json;
using Neptune.NsPay.PayOrderDeposits;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using SqlSugar;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class PayOrderService : BaseService<PayOrder>, IPayOrderService, IDisposable
    {
        public PayOrderService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
  
        }

        public async Task<bool> UpsertPayOrder(List<PayOrder> datalist)
        {
            try
            {
                var sp = "Upsert_PayOrder";
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

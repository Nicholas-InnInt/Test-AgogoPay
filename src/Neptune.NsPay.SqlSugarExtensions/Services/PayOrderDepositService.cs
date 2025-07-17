using Abp.Json;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.PayOrderDeposits;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using SqlSugar;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class PayOrderDepositService : BaseService<PayOrderDeposit>, IPayOrderDepositService, IDisposable
    {
        public PayOrderDepositService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }


        public async Task<bool> UpsertPayOrderDeposit(List<PayOrderDeposit> datalist)
        {
            try
            {
                var sp = "Upsert_PayOrderDeposits";
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

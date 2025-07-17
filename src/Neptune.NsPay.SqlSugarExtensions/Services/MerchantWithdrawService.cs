using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using System.Diagnostics;


namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class MerchantWithdrawService : BaseService<MerchantWithdraw>, IMerchantWithdrawService, IDisposable
    {
        public MerchantWithdrawService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<List<MerchantWithdraw>> GetWithdrawalByDateRange(DateTime startDate, DateTime endDate, MerchantWithdrawStatusEnum status)
        {
            List<MerchantWithdraw> merchantWithdraws = null;

            try
            {
                merchantWithdraws = Db.Queryable<MerchantWithdraw>().Where(x => x.ReviewTime >= startDate && x.ReviewTime <= endDate && x.Status == status).ToList();
            }
            catch(Exception ex)
            {

            }

            return merchantWithdraws;
        }

        public void Dispose()
        {
        }
    }
}

using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.WithdrawalDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class WithdrawalDevicesService : BaseService<WithdrawalDevice>, IWithdrawalDevicesService, IDisposable
    {
        public WithdrawalDevicesService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }


        public async Task<List<WithdrawalDevice>> GetWithdrawDevices(string merchantCode)
        {
            var sql = "";
            if (string.IsNullOrEmpty(merchantCode))
            {
                sql = "SELECT [Id],[Name],[Phone],[Model],[Status],[Process],[CreationTime],[CreatorUserId],[LastModificationTime],[LastModifierUserId],[IsDeleted],[DeleterUserId],[DeletionTime],[BankOtp],[LoginPassWord],[BankType],[ArrangementId],[CardName],[MerchantCode] FROM [dbo].[WithdrawalDevices] WITH (NOLOCK) where Status=1 and Process=0 and MerchantCode=''";
            }
            else
            {
                sql = "SELECT [Id],[Name],[Phone],[Model],[Status],[Process],[CreationTime],[CreatorUserId],[LastModificationTime],[LastModifierUserId],[IsDeleted],[DeleterUserId],[DeletionTime],[BankOtp],[LoginPassWord],[BankType],[ArrangementId],[CardName],[MerchantCode] FROM [dbo].[WithdrawalDevices] WITH (NOLOCK) where Status=1 and Process=0 and MerchantCode='" + merchantCode + "'";
            }
            var payGroup = await Db.Ado.SqlQueryAsync<WithdrawalDevice>(sql);

            if (payGroup != null)
            {
                return payGroup;
            }
            return new List<WithdrawalDevice>();
        }

        public void Dispose()
        {
        }
    }
}

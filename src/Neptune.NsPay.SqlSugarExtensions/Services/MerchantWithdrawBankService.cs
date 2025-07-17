using Neptune.NsPay.MerchantSettings;
using Neptune.NsPay.MerchantWithdrawBanks;
using Neptune.NsPay.SqlSugarExtensions.DbContext;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public class MerchantWithdrawBankService: BaseService<MerchantWithdrawBank>, IMerchantWithdrawBankService
    {
        public MerchantWithdrawBankService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}

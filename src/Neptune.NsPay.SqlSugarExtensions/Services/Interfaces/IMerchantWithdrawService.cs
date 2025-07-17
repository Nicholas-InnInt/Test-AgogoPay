using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalOrders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SqlSugarExtensions.Services.Interfaces
{
    public interface IMerchantWithdrawService: IBaseService<MerchantWithdraw>
    {
        Task<List<MerchantWithdraw>> GetWithdrawalByDateRange(DateTime startDate , DateTime endDate , MerchantWithdrawStatusEnum status);
    }
}

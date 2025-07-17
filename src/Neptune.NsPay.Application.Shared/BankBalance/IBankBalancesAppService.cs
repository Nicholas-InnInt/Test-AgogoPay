using Neptune.NsPay.BankBalance.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.BankBalance
{
    public interface IBankBalancesAppService
    {
        Task<List<GetAllBankBalancesViewDto>> GetBankBalance(GetAllBankBalanceInput input);
    }
}

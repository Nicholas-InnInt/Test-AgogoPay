using Neptune.NsPay.BankBalance.Dto;
using System.Collections.Generic;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.BankBalance
{
    public class BankBalancesViewModel
    {
        public string FilterText { get; set; }

        public List<GetAllBankBalancesViewDto> BankBalances { get; set; }
    }
}

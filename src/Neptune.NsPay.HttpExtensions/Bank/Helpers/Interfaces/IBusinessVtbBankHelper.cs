using Neptune.NsPay.HttpExtensions.Bank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface IBusinessVtbBankHelper
    {
        Task<List<BusinessVtbBankTransactionsItem>> GetTransactionHistory(string account, string cardNumber);
        BusinessVtbBankLoginModel GetSessionId(string account);
        void SetSessionId(string account, BusinessVtbBankLoginModel sessionid);
        void RemoveToken(string account);
        string GetLastRefNoKey(string account);
        void SetLastRefNoKey(string account, string refno);
    }
}

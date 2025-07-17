using Neptune.NsPay.HttpExtensions.Bank.Models;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface IBusinessMBBankHelper
    {
        Task<List<BusinessMBBankTransactionHistoryList>> GetTransactionHistory(string account, string cardNumber, string name);
        BusinessMBBankLoginModel GetSessionId(string account);
        void RemoveToken(string account);
        void SetSessionId(string account, BusinessMBBankLoginModel sessionid);
        string GetLastRefNoKey(string account);
        void SetLastRefNoKey(string account, string refno);
    }
}

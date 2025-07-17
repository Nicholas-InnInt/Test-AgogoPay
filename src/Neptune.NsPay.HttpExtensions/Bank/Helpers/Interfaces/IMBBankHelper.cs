
using Neptune.NsPay.HttpExtensions.Bank.Models;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface IMBBankHelper
    {
        string GetLogin(string userid, string password, string captcha);
        Task<List<TransactionHistoryItem>> GetTransactionHistory(string account, string cardNumber);
        MbBankLoginModel GetSessionId(string account);

        void RemoveSessionId(string cardNumber);

        void SetSessionId(string account, MbBankLoginModel sessionid);
        string GetLastRefNoKey(string account);
        void SetLastRefNoKey(string account, string refno);
    }
}

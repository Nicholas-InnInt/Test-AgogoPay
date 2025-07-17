
using Neptune.NsPay.HttpExtensions.Bank.Models;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface IVietinBankHelper
    {
        string GetLogin(string userid, string password, string captchaCode, string captchaId);
        Task<List<HisTransactionItem>> GetHistTransactions(string account, string cardNumber);
        string GetSessionId(string account);
        VtbBankLoginModel GetVtbBankLoginModel(string account);

        void RemoveSessionId(string account);
        void SetSessionId(string account, VtbBankLoginModel sessionid);
        string GetLastRefNoKey(string account);
        void SetLastRefNoKey(string account, string refno);

    }
}

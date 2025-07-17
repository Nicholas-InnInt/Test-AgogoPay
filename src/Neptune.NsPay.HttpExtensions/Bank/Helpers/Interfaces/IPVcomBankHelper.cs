
using Neptune.NsPay.HttpExtensions.Bank.Models;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface IPVcomBankHelper
    {
        Task<bool> Verify(string account);
        Task<List<PVcomBankTransactionHistoryList>> GetHistoryAsync(string account, string cardNo);
        Task<PVcomBankAccountResponse> GetQueryAccountList(string account,string cardNo);
        PVcomBankLoginModel GetSessionId(string account);
        void SetSessionId(string account, PVcomBankLoginModel sessionid);
        void RemoveSessionId(string account);
        string GetLastRefNoKey(string account);
        void SetLastRefNoKey(string account, string refno);
    }
}


using Neptune.NsPay.HttpExtensions.Bank.Models;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface IVietcomBankHelper
    {
        Task<VietcomBankLoginResponse> GetFirstLoginAsync(VietcomLoginRequest input, string cardNo, string bankApiUrl);
        Task<VietcomBankLoginResponse> GetSecondLoginAsync(VietcomLoginRequest input, string cardNo, string bankApiUrl);
        Task<ViecomBankTransactionHistoryResponse> GetTransactionHistory(string account, string cardnumber, string bankApiUrl);
        Task<List<ViecomBankTransactionHistoryItem>> GetTransactionHistoryByPage(string account, string cardnumber, int pages, VietcomBankLoginResponse cacheToken, string bankApiUrl);
        Task<decimal> GetBalance(string account, string cardnumber, string bankApiUrl);
        bool Verify(string account);
        VietcomBankLoginResponse GetSessionId(string account);
        void SetSessionId(string account, VietcomBankLoginResponse sessionid);
        void RemoveSessionId(string account);
        void ReplaceSessionId(string account, VietcomBankLoginResponse sessionid);
        string GetLastRefNoKey(string account);
        void SetLastRefNoKey(string account, string refno);

    }
}

using Neptune.NsPay.HttpExtensions.Bank.Models;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface IAcbBankHelper
    {
        //Task<AcbBankTransactionHistoryResponse> GetHistoryListAsync(string account, string cardno, string bankApiUrl, int page, int totalpage, int type);
        Task<List<AcbBankTransactionHistoryData>> GetHistoryAsync(string account, string cardno, string bankApiUrl, int type);
        Task<List<AcbBankTransactionHistoryData>> GetBusinessHistoryAsync(string account, string cardno, string bankApiUrl, int type);
        Task<AcbBankTransactionHistoryResponse> GetLoginAsync(string account, string cardno, string cookie, string bankApiUrl, int type);
        
        AcbBankLoginModel GetSessionId(string account);
        void SetSessionId(string account, AcbBankLoginModel sessionid);
        void RemoveSessionId(string account);
        void ReplaceSessionId(string account, AcbBankLoginModel sessionid);
        string GetLastRefNoKey(string account);
        void SetLastRefNoKey(string account, string refno);

    }
}

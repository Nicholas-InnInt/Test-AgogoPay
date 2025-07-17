
using Neptune.NsPay.HttpExtensions.Bank.Models;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface IBidvBankHelper
    {
        Task<BidvBankLoginResponse> GetFirstLogin(string card, string userid, string password, string captcha, string captchaToken, string bankApiUrl);
        Task<BidvBankLoginResponse> GetSecondLoginAsync(string card, string userid, string token, string bankApiUrl);
        Task<BidvBankTransactionHistoryResponse> GetHistoryAsync(string account, string cardNo, string bankApiUrl);
        Task<BidvBankTransactionHistoryResponse> GetHistoryByPageAsync(string account, string cardNo, string nextRunbal, string postingDate, string postingOrder, string bankApiUrl);
        BidvBankLoginResponse GetSessionId(string account);
        void SetSessionId(string account, BidvBankLoginResponse sessionid);
        void RemoveSessionId(string account);
        string GetLastRefNoKey(string account);
        void SetLastRefNoKey(string account, string refno);
    }
}

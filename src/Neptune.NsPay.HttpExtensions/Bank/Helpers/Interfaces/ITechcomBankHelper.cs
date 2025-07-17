
using Neptune.NsPay.HttpExtensions.Bank.Models;

namespace Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces
{
    public interface ITechcomBankHelper
    {
        Task<bool> Verify(string account);
        Task<List<TechcomBankTransactionHistoryResponse>> GetTransactionHistory(string account);
        Task<List<TechcomBankTransactionHistoryResponse>> GetBusinessTransactionHistory(string cardno, string account);
        Task<TechcomBankTransferResult> CreateTransfer(string account, TechcomBankTransferRequest transferRequest);
        string GetTransactionCookie(string cacheCookie, string accessToken, string sessionstate);
        TechcomBankLoginToken GetToken(string account);
        void SetToken(string account, TechcomBankLoginToken token);
        BusinessTechcomBankLoginToken GetBusinessToken(string account);
        void SetBusinessToken(string account, BusinessTechcomBankLoginToken token);
        void RemoveBusinessToken(string account);
        void RemoveToken(string account);
        void ReplaceToken(string account, TechcomBankLoginToken token);
        string GetLastRefNoKey(string account);
        void SetLastRefNoKey(string account, string refno);
        string GetBusinessLastRefNoKey(string account);
        void SetBusinessLastRefNoKey(string account, string refno);
    }
}

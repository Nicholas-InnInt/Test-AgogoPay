using Neptune.NsPay.HttpExtensions.Bank.Enums;

namespace Neptune.NsPay.HttpExtensions.Bank.Models
{
    public class MbBankLoginModel
    {
        public string SessionId { get; set; }
        public int ErrorCount { get; set; }
    }
    public class BaseResponse
    {
        public string message { get; set; }
        public bool ok { get; set; }
        public string responseCode { get; set; }
    }

    #region 验证码
    public class CaptchaImageRequest
    {
        public string deviceIdCommon { get; set; }
        public string refNo { get; set; }
        public string sessionId { get; set; }
    }

    public class CaptchaImageResponse
    {
        public string imageString { get; set; }
        public string refNo { get; set; }
        public BaseResponse result { get; set; }
    }

    #endregion

    #region 登录

    public class LoginRequest
    {
        public string userId { get; set; }
        public string password { get; set; }
        public string captcha { get; set; }
        public string sessionId { get; set; }
        public string refNo { get; set; }
        public string deviceIdCommon { get; set; }
    }

    public class LoginResponse
    {
        public string refNo { get; set; }
        public BaseResponse result { get; set; }
        public string sessionId { get; set; }
    }

    #endregion

    #region 账单列表

    public class TransactionHistoryRequest
    {
        public string accountNo { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
        //public string historyNumber { get; set; }
        //public string historyType { get; set; }
        //public string type { get; set; }
        public string sessionId { get; set; }
        public string refNo { get; set; }
        public string deviceIdCommon { get; set; }
    }

    public class TransactionHistoryResponse
    {
        public string refNo { get; set; }
        public BaseResponse result { get; set; }

        public List<TransactionHistoryItem> transactionHistoryList { get; set; }
    }

    public class TransactionHistoryItem
    {
        public string transactionDate { get; set; }
        public string accountNo { get; set; }
        public string creditAmount { get; set; }
        public string debitAmount { get; set; }
        public string description { get; set; }
        public string availableBalance { get; set; }
        public string refNo { get; set; }
        public string benAccountName { get; set; }
        public string bankName { get; set; }
        public string benAccountNo { get; set; }
    }

    public class TransactionHistoryDetail : TransactionHistoryItem
    {
        public DateTime TransactionTime { get; set; }
    }

    #endregion


}

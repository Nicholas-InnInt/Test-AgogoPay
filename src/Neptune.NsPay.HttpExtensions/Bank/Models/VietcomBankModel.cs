using Neptune.NsPay.HttpExtensions.Bank.Enums;

namespace Neptune.NsPay.HttpExtensions.Bank.Models
{
    public class VietcomLoginRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string captchaToken { get; set; }
        public string captchaValue { get; set; }
        public string codeotp { get; set; }
    }

    public class VietcomFirstLoginResponse
    {
        public int code { get; set; }
        public VietcomFirstLoginData data { get; set; }
    }
    public class VietcomFirstLoginData
    {
        public string browserToken { get; set; }
        public string browserId { get; set; }
        public string tranId { get; set; }
        public string user { get; set; }
        public string challenge { get; set; }
    }

    public class VietcomRequestModel
    {
        public string d { get; set; }
        public string k { get; set; }
    }

    public class VietcomSecondLoginResponse
    {
        public int code { get; set; }
        public VietcomBankLoginResponseUserInfo data { get; set; }
    }

    public class VietcomBankLoginResponse
    {
        public string Account { get; set; }
        public int code { get; set; }
        public VietcomBankLoginResponseUserInfo userInfo { get; set; }
        public VietcomFirstLoginData loginCache { get; set; }

        public DateTime CreateTime { get; set; }
    }

    public class VietcomBankLoginResponseUserInfo
    {
        public string cif { get; set; }
        public string user { get; set; }
        public string mobileId { get; set; }
        public string clientId { get; set; }
        public string sessionId { get; set; }
    }

    public class VietcomBankLoginToken
    {
        public string cif { get; set; }
        public string user { get; set; }
        public string mobileId { get; set; }
        public string clientId { get; set; }
        public string sessionId { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class ViecomBankTransactionHistoryResponse
    {
        public int code { get; set; }
        public List<ViecomBankTransactionHistoryItem> data { get; set; }
    }
    public class ViecomBankTransactionHistoryItem
    {
        public string Reference { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
        public string DorCCode { get; set; }
        public string PostingDate { get; set; }
        public string PostingTime { get; set; }
        public string Teller { get; set; }
    }

    public class ViecomBankTransactionHistoryItemTime : ViecomBankTransactionHistoryItem
    {
        public DateTime TransactionTime { get; set; }
    }

    public class VietcomBankBalance
    {
        public int code { get; set; }
        public string balance { get; set; }
    }

    #region 出款

    public class VietcomBankCheckCardResponse
    {
        public int code { get; set; }
        public VietcomBankCheckCardData data { get; set; }
    }
    public class VietcomBankCheckCardData
    {
        public string cardHolderName { get; set; }
        public string bankCodde { get; set; }
        public string des { get; set; }
    }


    public class VietcomBankTransferRequest
    {
        public string OrderId { get; set; }
        public string AccountNo { get; set; }
        public string Name { get; set; }
        public string Money { get; set; }
        public string BenAccountNo { get; set; }
        public string BenAccountName { get; set; }
        public string BankName { get; set; }
        public string Remark { get; set; }
    }



    public class VietcomBankPayMentOrdersResponse
    {
        public string id { get; set; }
        public string reasonText { get; set; }
        public string createdBy { get; set; }
        public string createdAt { get; set; }
        public string updatedBy { get; set; }
        public string updatedAt { get; set; }
    }

    public class VietcomBankTransferResult
    {
        public TransferResultTypeEnums Code { get; set; }
        public VietcomBankTransferResponse Data { get; set; }
    }

    public class VietcomBankTransferResponse
    {
        public int code { get; set; }
        public string tranId { get; set; }
        public string challenge { get; set; }
    }

    public class VietcomBankTransferConfimResponse
    {
        public int code { get; set; }
    }

    #endregion

}

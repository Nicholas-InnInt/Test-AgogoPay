
namespace Neptune.NsPay.HttpExtensions.Bank.Models
{
    public class VtbBankLoginModel
    {
        public string Account { get; set; }
        public string sessionid { get; set; }
    }

    public class VietinBankLoginEncryptedRequest
    {
        public string encrypted { get; set; }
    }

    #region 登录

    public class VietinBankLoginRequest
    {
        public string accessCode { get; set; }
        public string browserInfo { get; set; }
        public string captchaCode { get; set; }
        public string captchaId { get; set; }
        public string clientInfo { get; set; }
        public string lang
        {
            get
            {
                return "vi";
            }
        }
        public string requestId { get; set; }
        public string userName { get; set; }
        public string signature { get; set; }
    }

    public class VietinBankLoginResponse
    {
        public string requestId { get; set; }
        public string sessionId { get; set; }
        public bool error { get; set; }
        public string systemDate { get; set; }
        public string status { get; set; }
        public string customerNumber { get; set; }
        public string ipayId { get; set; }
        public string addField3 { get; set; }
        public string tokenId { get; set; }
    }

    #endregion

    #region 历史订单

    public class HistTransactionsRequest
    {
        public string accountNumber { get; set; }
        public string endDate { get; set; }
        public string startDate { get; set; }
        public string lang
        {
            get
            {
                return "en";
            }
        }
        public int maxResult
        {
            get
            {
                return 999999999;
            }
        }
        public int pageNumber
        {
            get
            {
                return 0;
            }
        }
        public string requestId { get; set; }
        public decimal searchFromAmt { get; set; }
        public decimal searchToAmt { get; set; }
        public string searchKey { get; set; }
        public string sessionId { get; set; }
        public string tranType { get; set; }
        public string signature { get; set; }
    }

    public class HistTransactionResponse
    {
        public string requestId { get; set; }
        public string sessionId { get; set; }
        public bool error { get; set; }
        public string accountNo { get; set; }
        public int currentPage { get; set; }
        public int nextPage { get; set; }
        public int pageSize { get; set; }
        public int totalRecords { get; set; }
        public string warningMsg { get; set; }
        public List<HisTransactionItem> transactions { get; set; }
    }


    public class HisTransactionItem
    {
        public string remark { get; set; }
        public decimal amount { get; set; }
        public decimal balance { get; set; }
        public string trxId { get; set; }
        public string processDate { get; set; }
        public string dorC { get; set; }//C 表示存款 D 表示取款
        public string corresponsiveAccount { get; set; }
        public string corresponsiveName { get; set; }
        public DateTime TransactionTime { get; set; }
    }

    #endregion 

}

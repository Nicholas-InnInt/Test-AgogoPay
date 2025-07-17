using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.Bank.Models
{
    public class BidvFirstLoginResponse
    {
        public int code { get; set; }
        public BidvFirstResponseData data { get; set; }
    }

    public class BidvFirstResponseData
    {
        public string token { get; set; }
        public string user { get; set; }
    }

    public class BidvSeconLoginResponse
    {
        public int code { get; set; }
        public BidvSeconLoginResponseData data { get; set; }
    }

    public class BidvSeconLoginResponseData
    {
        public string user { get; set; }
    }

    public class BidvBankLoginResponse
    {
        public string Account { get; set; }
        public int code { get; set; }
        public BidvSeconLoginResponse userInfo { get; set; }
        public BidvFirstLoginResponse loginCache { get; set; }

        public PaySessionBidvModel paySessionModel { get; set; }
        public string token { get; set; }
        public DateTime Createtime { get; set; }
    }

    public class PaySessionBidvModel
    {
        public string BrowserId { get; set; }
        public string ClientId { get; set; }
        public string AppVersionId { get; set; }
        public string Token { get; set; }
        public string Cookie { get; set; }
    }

    public class BidvLoginResponseModel
    {
        public int mid { get; set; }
        public string code { get; set; }
        public string user { get; set; }
        public string des { get; set; }
    }

    public class BidvAesRequestModel
    {
        public string d { get; set; }
        public string k { get; set; }
    }

    public class BidvBankTransactionHistoryResponse
    {
        public int code { get; set; }
        public List<BidvBankTransactionHistoryItem> data { get; set; }

        public string nextRunbal { get; set; }
        public string postingOrder { get; set; }
        public string postingDate { get; set; }

    }
    public class BidvBankTransactionHistoryItem
    {
        public string txnDate { get; set; }
        public string txnTime { get; set; }
        public string txnRemark { get; set; }
        public string txnType { get; set; }
        public string amount { get; set; }
        public string refNo { get; set; }
        public string balance { get; set; }
    }

    public class BidvBankTransactionHistoryItemTime : BidvBankTransactionHistoryItem
    {
        public DateTime TransactionTime { get; set; }
    }

}

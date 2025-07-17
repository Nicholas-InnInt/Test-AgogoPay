using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.Bank.Models
{
    public class BusinessMBBankLoginModel
    {
        public string SessionId { get; set; }
        public string BizPlatform { get; set; }
        public string BizTraceId { get; set; }
        public string BizTracking { get; set; }
        public string BizVersion { get; set; }
        public string ElasticApmTraceparent { get; set; }
        public string UserAgent { get; set; }

        public DateTime CreateTime { get; set; }
    }

    public class BusinessMBBankTransactionHistoryResponse
    {
        public string refNo { get; set; }
        public BusinessMBBankTransactionHistoryResult result { get; set; }
        public List<BusinessMBBankTransactionHistoryList> transactionHistoryList { get; set; }
    }
    public class BusinessMBBankTransactionHistoryResult
    {
        public string message { get; set; }
        public string responseCode { get; set; }
        public bool ok { get; set; }
    }

    public class BusinessMBBankTransactionHistoryList
    {
        public string postingDate { get; set; }
        public string transactionDate { get; set; }
        public string accountNo { get; set; }
        public decimal creditAmount { get; set; }
        public decimal debitAmount { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public decimal availableBalance { get; set; }
        public string beneficiaryAccount { get; set; }
        public string transactionRefNo { get; set; }
        public int rowIndex { get; set; }
        public string floatBal { get; set; }
        public string openBalance { get; set; }
        public string closeBalance { get; set; }
        public string accountName { get; set; }
        public string stmtId { get; set; }
        public string channel { get; set; }
        public string vaNo { get; set; }
        public string vaName { get; set; }

        public DateTime TransactionTime { get; set; }
    }
}

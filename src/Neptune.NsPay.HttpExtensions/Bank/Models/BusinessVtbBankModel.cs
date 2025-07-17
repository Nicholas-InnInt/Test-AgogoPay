using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.Bank.Models
{

    public class BusinessVtbBankLoginModel
    {
        public string requestId { get; set; }
        public string cifno { get; set; }
        public string screenResolution { get; set; }
        public string sessionId { get; set; }
        public string username { get; set; }

        public DateTime CreateTime { get; set; }
    }

    public class BusinessVtBankTransactionHistoryResponse
    {
        public string requestId { get; set;  }
        public string sessionId { get; set; }
        public BusinessVtbBankStatus status { get; set; }
        public string accountNo { get; set; }
        public string currency { get; set; }
        public int currentPage { get; set; }
        public int nextPage { get; set; }
        public int pageSize { get; set; }
        public string accountType { get; set; }
        public List<BusinessVtbBankTransactionsItem> transactions { get; set; }
    }
    public class BusinessVtbBankStatus
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class BusinessVtbBankTransactionsItem
    {
        public string tranDate { get; set; }
        public string remark { get; set; }
        public string amount { get; set; }
        public string balance { get; set; }
        public string currency { get; set; }
        public string trxId { get; set; }
        public string dorc { get; set; }
        public string branchId { get; set; }
        public string branchName { get; set; }
        public string channel { get; set; }
        public string corresponsiveAccount { get; set; }
        public string corresponsiveName { get; set; }
        public string trxRefNo { get; set; }
        public string pmtType { get; set; }
        public string trnNum { get; set; }
        public string endAmt { get; set; }
        public string beginAmt { get; set; }
        public int numberOrder { get; set; }
        public string beneficiaryBankId { get; set; }
        public string beneficiaryBankName { get; set; }
        public string transferBankName { get; set; }
        public string businessDate { get; set; }
        public string originalAmt { get; set; }
        public string originalCurrency { get; set; }
        public string tranTypeName { get; set; }
        public string bankType { get; set; }
        public string pmtId { get; set; }
        public string trnSrc { get; set; }
        public string tellerId { get; set; }
        public string customerCode { get; set; }

        public DateTime TransactionTime { get; set; }
    }
}

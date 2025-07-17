using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.Bank.Models
{
    public class AcbBankLoginModel
    {
        public string Cookies { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public int ErrorCount { get; set; } = 0;
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
    }

    public class AcbBankTransactionHistoryResponse
    {
        public int code { get; set; }
        public string balance { get; set; }

        public List<AcbBankTransactionHistoryData> data { get; set; }

        public int totalPage { get; set; } = 0;
    }

    public class AcbBankRefreshResponse
    {
        public int code { get; set; }
    }

    public class AcbBankTransactionHistoryData
    {
        public string id { get; set; }
        public string desc { get; set; }
        public string money { get; set; }
        public string balance { get; set; }
    }
    public class AcbBankTransactionHistoryDataDetail : AcbBankTransactionHistoryData
    {
        public string TxnType { get; set; }
    }
}

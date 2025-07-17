using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neptune.NsPay.Commons;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
   
    [Collection("merchantfundslog")]
    public class MerchantFundsLogMongoEntity : BaseMongoEntityNoTransactionTime
    {
        public string MerchantFundId { get; set; }
        public int MerchantId { get; set; }
        public string MerchantCode { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public decimal TranferAmount { get; set; }
        public decimal RateFeeBalance { get; set; }
        public decimal Balance { get; set; }
        public decimal FrozenBalance { get; set; }
        public long FundVersionNumber { get; set; }
        public bool IsCompleted { get; set; } // Indicate already inserted into merchant Bills
        public string DetailsJson { get; set; }
        public string ChangesDetailsJson { get; set; } // change serialize to UpdateFundResult
        public CashLogSourceType SourceType { get; set; } // 1 . Frozen Withdrawal Amount  2 . Frozen Merchant Withdrawal 
        public string ReferenceId { get; set; } // Unique Identified Amount Merchant Records
    }
}


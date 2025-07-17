using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
    [Collection("merchantfunds")]
    public class MerchantFundsMongoEntity : BaseMongoEntityNoTransactionTime
    {
        public int MerchantId { get; set; }
        public string MerchantCode { get; set; }

        #region Default VND

        public decimal DepositAmount { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public decimal TranferAmount { get; set; }
        public decimal RateFeeBalance { get; set; }
        public decimal Balance { get; set; }
        public decimal FrozenBalance { get; set; }

        #endregion Default VND

        #region USDT

        public decimal USDTDepositAmount { get; set; }
        public decimal USDTWithdrawalAmount { get; set; }
        public decimal USDTTranferAmount { get; set; }
        public decimal USDTRateFeeBalance { get; set; }
        public decimal USDTBalance { get; set; }
        public decimal USDTFrozenBalance { get; set; }

        #endregion USDT

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UpdateTime { get; set; }

        public long UpdateUnixTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastPayOrderTransactionTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastWithdrawalOrderTransactionTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastMerchantWithdrawalTransactionTime { get; set; }

        public long? VersionNumber { get; set; }
    }
}
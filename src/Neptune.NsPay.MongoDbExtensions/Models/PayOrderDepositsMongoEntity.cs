using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
    [Collection("payorderdeposit")]
    public class PayOrderDepositsMongoEntity : BaseMongoEntityNoTransactionTime
    {
        public string RefNo { get; set; }
        //public string BankOrderId { get; set; }
        public int PayType { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal AvailableBalance { get; set; }
        public string CreditBank { get; set; }
        public string CreditAcctNo { get; set; }
        public string CreditAcctName { get; set; }
        public string DebitBank { get; set; }
        public string DebitAcctNo { get; set; }
        public string DebitAcctName { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TransactionTime { get; set; }
        public string OrderId { get; set; }
        public int MerchantId { get; set; }
        public string MerchantCode { get; set; }
        public string RejectRemark {  get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string AccountNo { get; set; }
        public int PayMentId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? OperateTime { get; set; }
    }
}

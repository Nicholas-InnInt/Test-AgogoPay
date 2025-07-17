using MongoDB.Entities;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
    [Collection("merchantbills")]
    public class MerchantBillsMongoEntity : BaseMongoEntity
    {
        public int MerchantId { get; set; }
        public string? MerchantCode { get; set; }
        public string? BillNo { get; set; }
        public MerchantBillTypeEnum BillType { get; set; }
        public PayMentMethodEnum? MoneyType { get; set; }
        public decimal Money { get; set; }
        public decimal Rate { get; set; }
        public decimal FeeMoney { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string PlatformCode { get; set; }
        public string Remark { get; set; }
        public long OrderUnixTime { get; set; }
        public DateTime OrderTime { get; set; }
        public long BalanceVersion { get; set; }
    }
}
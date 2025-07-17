namespace Neptune.NsPay.RedisExtensions.Models
{
    public class PayOrderDepositSuccessModel
    {
        //0--存款，1--拒绝订单
        public string DepositsId { get; set; }
        public int Type { get; set; }
        public string BankOrderId { get; set; }
        public int MerchantId { get; set; }
        public string MerchantCode { get; set; }
        public int PayMentId { get; set; }
        public string OrderId { get; set; }
        public long Userid { get; set; }
        public string TransactionNo { get; set; }
        public decimal TradeMoney { get; set; }
        public string Remark { get; set; }

        public DateTime CreateTime { get; set; }
    }
}

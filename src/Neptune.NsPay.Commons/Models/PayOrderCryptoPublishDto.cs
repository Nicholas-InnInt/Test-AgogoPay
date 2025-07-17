using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.Commons.Models
{
    public class PayOrderCryptoPublishDto
    {
        public string ProcessId { get; set; }
        public string MerchantCode { get; set; }
        public PayMentTypeEnum PaymentType { get; set; }
        public List<string> PayOrderDepositIds { get; set; }
        public string UpdateOrderNumberBillOnly { get; set; }
    }
}
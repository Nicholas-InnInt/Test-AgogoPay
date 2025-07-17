using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.RedisExtensions.Models
{
	public class BankOrderPubModel
	{
		public string? MerchantCode { get; set; }
		public PayMentTypeEnum Type { get; set; }
		public int PayMentId { get; set; } 
		public string PayOrderId { get; set; } 
		public string Id { get; set; }
		public decimal Money { get; set; }
		public string Desc { get; set; }
	}

}

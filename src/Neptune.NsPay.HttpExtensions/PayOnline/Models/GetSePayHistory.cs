namespace Neptune.NsPay.HttpExtensions.PayOnline.Models
{
	public class GetSePayHistoryRequest
	{
		public string memberId { get; set; }
		public string orderNumber { get; set; }
		public string sign { get; set; }
	}
	public class GetSePayHistoryResponse
	{
		public string amount { get; set; }
		public string tradeNo { get; set; }
		public string outTradeNo { get; set; }
		public string payTime { get; set; }
		public string status { get; set; }
		public string sign { get; set; }
		public string code { get; set; }
	}
}

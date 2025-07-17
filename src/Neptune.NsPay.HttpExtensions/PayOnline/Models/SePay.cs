namespace Neptune.NsPay.HttpExtensions.PayOnline.Models
{
	public class SePayRequest
	{
		public string memberId { get; set; }
		public string orderNumber { get; set; }
		public decimal amount { get; set; }
		public string callBackUrl { get; set; }
		public string payType { get; set; }
		public string playUserIp { get; set; }
		public string Key { get; set; }
	}

	public class SePayResponse
	{ 
		public string code { get; set; }
		public string result { get; set; }
		public string payUrl { get; set; }
		public string amount { get; set; }
		public string tradeNo { get; set; }
		public string payTime { get; set; }
		public string outTradeNo { get; set; }
		public string status { get; set; }
		public string sign { get; set; }
	}
}

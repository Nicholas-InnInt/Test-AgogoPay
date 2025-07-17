namespace Neptune.NsPay.HttpExtensions.PayOnline.Models
{
	public class SeCallbackModel
	{
		public string code { get; set; }
		public string result { get; set; }
		public string amount { get; set; }
		public string tradeNo { get; set; }
		public string payTime { get; set; }
		public string outTradeNo { get; set; }
		public string status { get; set; }
		public string sign { get; set; }
	}
}

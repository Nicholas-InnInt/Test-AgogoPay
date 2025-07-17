namespace Neptune.NsPay.HttpExtensions.PayOnline.Models
{
	public class FyPayRequest
	{
		public decimal Amount { get; set; }
		public string Channel { get; set; }
		public string Custom { get; set; }
		public string Notify_url { get; set; }
		public string Orderid { get; set; }
		public string Return_url { get; set; }
		public string Timestamp { get; set; }
		public string Uid { get; set; }
		public string UserIp { get; set; }
		public string Key { get; set; }
	}

	public class FyPayResponse
	{ 
		public string status { get; set; }
		public FyResultObject result { get; set; }
		public string sign { get; set; }
	}

	public class FyResultObject
	{ 
		public long transactionid { get; set; }
		public string payurl { get; set; }
		public float points { get; set; }
	}
}

namespace Neptune.NsPay.HttpExtensions.PayOnline.Models
{
	public class FyCallBackModel
	{
		public int status { get; set; }
		public FyCallBackResultObject result { get; set; }
		public string sign { get; set; }
	}

	public class FyCallBackResultObject 
	{ 
		public long transactionId { get; set; }
		public string orderid { get; set; }
		public decimal amount { get; set; }
		public decimal real_amount { get; set; }
		public string custom { get; set; }
	}
}

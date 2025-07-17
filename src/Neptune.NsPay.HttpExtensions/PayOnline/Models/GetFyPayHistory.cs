namespace Neptune.NsPay.HttpExtensions.PayOnline.Models
{
	public class GetFyPayHistoryRequest
	{
		public string Uid { get; set; }
		public string Timestamp { get; set; }
		public string Sign { get; set; }
		public string OrderId { get; set; }
	}
	public class GetFyPayHistoryResponse
	{
		public int status { get; set; }
		public FyPayResultObject result { get; set; }
		public string sign { get; set; }
	}

	public class FyPayResultObject
	{ 
		public int transactionid { get; set; }
		public string orderid { get; set; }
		public int channel { get; set; }
		public decimal amount { get; set; }
		public decimal real_amount { get; set; }
		public int status { get; set; }
		public string bdate { get; set; }
		public string cdate { get; set; }
	}
}

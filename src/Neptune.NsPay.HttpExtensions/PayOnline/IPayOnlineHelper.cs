using Neptune.NsPay.HttpExtensions.PayOnline.Models;

namespace Neptune.NsPay.HttpExtensions.PayOnline
{
	public interface IPayOnlineHelper
	{
		Task<SePayResponse> SePay(string baseUrl, SePayRequest request);
		Task<GetSePayHistoryResponse> GetSePayHistory(string baseUrl, GetSePayHistoryRequest request);
		Task<FyPayResponse> FyPay(string baseUrl, FyPayRequest request);
		Task<GetFyPayHistoryResponse> GetFyPayHistory(string baseUrl, GetFyPayHistoryRequest request);
	}
}

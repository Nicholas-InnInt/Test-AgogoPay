using Abp.Extensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.PayOnline.Models;
using Newtonsoft.Json;

namespace Neptune.NsPay.HttpExtensions.PayOnline
{
	public class PayOnlineHelper : IPayOnlineHelper
	{
		public async Task<SePayResponse> SePay(string baseUrl, SePayRequest request) 
		{
			try
			{
				var payUrl = "/api/order/pay/created";

				var url = baseUrl + payUrl;
				var signStr = $"amount={request.amount}&callBackUrl={request.callBackUrl}&memberId={request.memberId}&orderNumber={request.orderNumber}&payType={request.payType}&playUserIp={request.playUserIp}&key={request.Key}";
				var sign = MD5Helper.MD5Encrypt32(signStr).ToUpper();
				var formData = new Dictionary<string, string>
				{
					{ "memberId", request.memberId },
					{ "orderNumber", request.orderNumber },
					{ "amount", request.amount.ToString() },
					{ "callBackUrl", request.callBackUrl },
					{ "payType", request.payType },
					{ "sign", sign },
					{ "playUserIp", request.playUserIp}
				};

				var content = new FormUrlEncodedContent(formData);

				var response = await new HttpClient().PostAsync(url, content);
				var responseDetail = await response.Content.ReadAsStringAsync();
				if (!responseDetail.IsNullOrEmpty())
				{
					var detailResponse = JsonConvert.DeserializeObject<SePayResponse>(responseDetail);
					if (detailResponse == null)
					{
						return new SePayResponse() { code = "125" };
					}
					return detailResponse;
				}
				return new SePayResponse() { code = "125" };
			}
			catch (Exception ex)
			{
				NlogLogger.Error("Se支付 异常：", ex);
				return new SePayResponse() { code = "125" };
			}
		}
		public async Task<GetSePayHistoryResponse> GetSePayHistory(string baseUrl, GetSePayHistoryRequest request)
		{
			try
			{
				var payUrl = "/api/order/pay/query";

				var url = baseUrl + payUrl;
				var formData = new Dictionary<string, string>
				{
					{ "memberId", request.memberId },
					{ "orderNumber", request.orderNumber },
					{ "sign", request.sign }
				};

				var content = new FormUrlEncodedContent(formData);

				var response = await new HttpClient().PostAsync(url, content);
				var responseDetail = await response.Content.ReadAsStringAsync();
				if (!responseDetail.IsNullOrEmpty())
				{
					var detailResponse = JsonConvert.DeserializeObject<GetSePayHistoryResponse>(responseDetail);
					if (detailResponse == null)
					{
						return new GetSePayHistoryResponse() { code = "125" };
					}
					return detailResponse;
				}
				return new GetSePayHistoryResponse() { code = "125" };
			}
			catch (Exception ex)
			{
				NlogLogger.Error("Se支付查询 异常：", ex);
				return new GetSePayHistoryResponse() { code = "125" };
			}
		}
		public async Task<FyPayResponse> FyPay(string baseUrl, FyPayRequest request)
		{
			try
			{
				var payUrl = "/pay";
				var url = baseUrl + payUrl;
				var signStr = $"amount={request.Amount}&channel={request.Channel}&custom={request.Custom}&notify_url={request.Notify_url}&orderid={request.Orderid}&return_url={request.Return_url}&timestamp={request.Timestamp}&uid={request.Uid}&userip={request.UserIp}&key={request.Key}";
				var sign = MD5Helper.MD5Encrypt32(signStr).ToUpper();

				var multipartFormDataContent = new MultipartFormDataContent
				{
					{ new StringContent(request.Uid), "uid" },
					{ new StringContent(request.Orderid), "orderid" },
					{ new StringContent(request.Channel), "channel" },
					{ new StringContent(request.Notify_url), "notify_url" },
					{ new StringContent(request.Return_url), "return_url" },
					{ new StringContent(request.Amount.ToString()), "amount" },
					{ new StringContent(request.UserIp), "userip" },
					{ new StringContent(request.Timestamp), "timestamp" },
					{ new StringContent(request.Custom), "custom" },
					{ new StringContent(sign), "sign" }
				};
				var response = await new HttpClient().PostAsync(url, multipartFormDataContent);
				var responseDetail = await response.Content.ReadAsStringAsync();
				if (!responseDetail.IsNullOrEmpty())
				{
					var detailResponse = JsonConvert.DeserializeObject<FyPayResponse>(responseDetail);
					if (detailResponse == null)
					{
						return new FyPayResponse() { status = "125" };
					}
					return detailResponse;
				}
				return new FyPayResponse() { status = "125" };
			}
			catch (Exception ex)
			{
				NlogLogger.Error("Fy支付 异常：", ex);
				return new FyPayResponse() { status = "125" };
			}
		}
		public async Task<GetFyPayHistoryResponse> GetFyPayHistory(string baseUrl, GetFyPayHistoryRequest request)
		{
			try
			{
				var payUrl = "/orderquery";

				var url = baseUrl + payUrl;
				var multipartFormDataContent = new MultipartFormDataContent
				{
					{ new StringContent(request.Uid), "uid" },
					{ new StringContent(request.Timestamp), "timestamp" },
					{ new StringContent(request.Sign), "sign" },
					{ new StringContent(request.OrderId), "orderid" }
				};
				var response = await new HttpClient().PostAsync(url, multipartFormDataContent);
				var responseDetail = await response.Content.ReadAsStringAsync();
				if (!responseDetail.IsNullOrEmpty())
				{
					var detailResponse = JsonConvert.DeserializeObject<GetFyPayHistoryResponse>(responseDetail);
					if (detailResponse == null)
					{
						return new GetFyPayHistoryResponse() { status = 125 };
					}
					return detailResponse;
				}
				return new GetFyPayHistoryResponse() { status = 125 };
			}
			catch (Exception ex)
			{
				NlogLogger.Error("Fy支付查询 异常：", ex);
				return new GetFyPayHistoryResponse() { status = 125 };
			}
		}

	}

}

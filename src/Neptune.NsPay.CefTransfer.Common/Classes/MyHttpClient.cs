using Neptune.NsPay.CefTransfer.Common.Models;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalOrders;
using Newtonsoft.Json;
using System.Text;

namespace Neptune.NsPay.CefTransfer.Common.Classes
{
    public class MyHttpClient
    {
        #region Variable
        #endregion

        #region Property
        public string BaseAddress { get; private set; }
        public string Sign { get; private set; } = "dcaebf01858f33594ff3074fb7b81d73";
        public string MerchantCode { get; private set; }
        public int @Type { get; private set; }
        public string Data { get; private set; }
        public string ErrorMessage { get; private set; }
        public HttpResponseMessage ResponseMessage { get; private set; }
        public HttpClient Client { get; private set; }
        public bool HasError { get { return !string.IsNullOrWhiteSpace(ErrorMessage); } }
        #endregion

        public MyHttpClient(string baseAddress, string merchantCode, int type)
        {
            BaseAddress = baseAddress;
            //Sign = sign;
            MerchantCode = merchantCode;
            Type = type;

            Client = new HttpClient();
            Client.BaseAddress = new Uri(BaseAddress);
        }
        #region Public
        public async Task<ResponseModel> LogUpdateAsync(string messages, DateTime logDate, string photoBase64, string paymentId, string phone)
        {
            var response = new ResponseModel();
            try
            {
                var model = new LogUpdateModel()
                {
                    Topic = "NsPayTransfer",
                    Message = messages,
                    DateTime = logDate,
                    Photo = photoBase64,
                    PaymentId = paymentId,
                    Phone = phone,
                };

                var reqData = JsonConvert.SerializeObject(model);
                response.MyRequestData = reqData;
                var content = new StringContent(reqData, Encoding.UTF8, "application/json");
                var url = "Log/Update";

                var client = new HttpClient();
                client.BaseAddress = new Uri("https://api.globallog.top/");
                client.DefaultRequestHeaders.Add("sign", "eec2eddc7e37448793c56d272ee2801d");
                var result = await client.PostAsync(url, content);
                response.MyRequestUri = result.RequestMessage.RequestUri;

                if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.MyIsSuccess = true;
                    var resStr = await result.Content.ReadAsStringAsync();
                    response = GetCommonResponse(response, resStr);
                    response.MyResponseString = resStr;
                }

                response.code = Convert.ToInt32(result?.StatusCode);
            }
            catch (Exception ex)
            {
                response.MyErrorMessage = ex.Message;
                response.MyExceptionMessage = ex.ToString();
            }
            return response;
        }

        public async Task<ResponseModel> GetDeviceAsync(string phone, WithdrawalDevicesBankTypeEnum bankType)
        {
            var response = new ResponseModel();
            try
            {
                var model = new GetDeviceModel()
                {
                    MerchantCode = MerchantCode,
                    Phone = phone,
                    BankType = (int)bankType,
                };

                var reqData = JsonConvert.SerializeObject(model);
                response.MyRequestData = reqData;
                var content = new StringContent(reqData, Encoding.UTF8, "application/json");
                var url = "Withdraw/GetDevice";

                var result = await Client.PostAsync(url, content);
                response.MyRequestUri = result.RequestMessage.RequestUri;

                if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.MyIsSuccess = true;
                    var resStr = await result.Content.ReadAsStringAsync();
                    response = GetCommonResponse(response, resStr);
                    response.MyResponseString = resStr;
                }

                response.code = Convert.ToInt32(result?.StatusCode);
            }
            catch (Exception ex)
            {
                response.MyErrorMessage = ex.Message;
                response.MyExceptionMessage = ex.ToString();
            }
            return response;
        }

        public async Task<ResponseModel> GetWithdrawOrderAsync(string phone, WithdrawalDevicesBankTypeEnum bankType)
        {
            var response = new ResponseModel();
            try
            {
                var model = new GetWithdrawOrderModel()
                {
                    MerchantCode = MerchantCode,
                    Phone = phone,
                    BankType = (int)bankType
                };

                var reqData = JsonConvert.SerializeObject(model);
                response.MyRequestData = reqData;
                var content = new StringContent(reqData, Encoding.UTF8, "application/json");
                var url = "Withdraw/GetWithdrawOrder";

                var result = await Client.PostAsync(url, content);
                response.MyRequestUri = result.RequestMessage.RequestUri;

                if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.MyIsSuccess = true;
                    var resStr = await result.Content.ReadAsStringAsync();
                    response = GetCommonResponse(response, resStr);
                    response.MyResponseString = resStr;
                }
            }
            catch (Exception ex)
            {
                response.MyErrorMessage = ex.Message;
                response.MyExceptionMessage = ex.ToString();
            }
            return response;
        }

        public async Task<ResponseModel> CheckWithdrawOrderAsync(string phone, WithdrawalDevicesBankTypeEnum bankType)
        {
            var response = new ResponseModel();
            try
            {
                var model = new CheckWithdrawOrderModel()
                {
                    MerchantCode = MerchantCode,
                    Phone = phone,
                    BankType = (int)bankType
                };

                var reqData = JsonConvert.SerializeObject(model);
                response.MyRequestData = reqData;
                var content = new StringContent(reqData, Encoding.UTF8, "application/json");
                var url = "Withdraw/CheckWithdrawOrder";

                var result = await Client.PostAsync(url, content);
                response.MyRequestUri = result.RequestMessage.RequestUri;

                if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.MyIsSuccess = true;
                    var resStr = await result.Content.ReadAsStringAsync();
                    response = GetCommonResponse(response, resStr);
                    response.MyResponseString = resStr;
                }
            }
            catch (Exception ex)
            {
                response.MyErrorMessage = ex.Message;
                response.MyExceptionMessage = ex.ToString();
            }
            return response;
        }

        public async Task<ResponseModel> UpdateWithdrawOrderAsync(string orderId, string transactionNo, WithdrawalOrderStatusEnum orderStatus, string remark)
        {
            var response = new ResponseModel();
            try
            {
                var model = new UpdateWithdrawOrderModel()
                {
                    OrderId = orderId,
                    TransactionNo = transactionNo,
                    OrderStatus = (int)orderStatus,
                    Remark = remark, // error message
                };

                var reqData = JsonConvert.SerializeObject(model);
                response.MyRequestData = reqData;
                var content = new StringContent(reqData, Encoding.UTF8, "application/json");
                var url = "Withdraw/UpdateWithdrawOrder";

                var result = await Client.PostAsync(url, content);
                response.MyRequestUri = result.RequestMessage.RequestUri;

                if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.MyIsSuccess = true;
                    var resStr = await result.Content.ReadAsStringAsync();
                    response = GetCommonResponse(response, resStr);
                    response.MyResponseString = resStr;
                }
            }
            catch (Exception ex)
            {
                response.MyErrorMessage = ex.Message;
                response.MyExceptionMessage = ex.ToString();
            }
            return response;
        }

        public async Task<ResponseModel> UpdateBalanceAsync(int deviceId, decimal balance)
        {
            var response = new ResponseModel();
            try
            {
                var model = new UpdateBalanceModel()
                {
                    DeviceId = deviceId,
                    Balance = balance
                };

                var reqData = JsonConvert.SerializeObject(model);
                response.MyRequestData = reqData;
                var content = new StringContent(reqData, Encoding.UTF8, "application/json");
                var url = "Withdraw/UpdateBalance";

                var result = await Client.PostAsync(url, content);
                response.MyRequestUri = result.RequestMessage.RequestUri;

                if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.MyIsSuccess = true;
                    var resStr = await result.Content.ReadAsStringAsync();
                    response = GetCommonResponse(response, resStr);
                    response.MyResponseString = resStr;
                }
            }
            catch (Exception ex)
            {
                response.MyErrorMessage = ex.Message;
                response.MyExceptionMessage = ex.ToString();
            }
            return response;
        }

        public async Task<ResponseModel> GetOrderOtpAsync(int deviceId, string orderId)
        {
            var response = new ResponseModel();
            try
            {
                var model = new GetOrderOtpModel()
                {
                    DeviceId = deviceId,
                    OrderId = orderId,
                };

                var reqData = JsonConvert.SerializeObject(model);
                response.MyRequestData = reqData;
                var content = new StringContent(reqData, Encoding.UTF8, "application/json");
                var url = "Withdraw/GetOrderOtp";

                var result = await Client.PostAsync(url, content);
                response.MyRequestUri = result.RequestMessage.RequestUri;

                if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.MyIsSuccess = true;
                    var resStr = await result.Content.ReadAsStringAsync();
                    response = GetCommonResponse(response, resStr);
                    response.MyResponseString = resStr; // OrderOtpModel
                }
            }
            catch (Exception ex)
            {
                response.MyErrorMessage = ex.Message;
                response.MyExceptionMessage = ex.ToString();
            }
            return response;
        }

        public async Task<ResponseModel> UpdateOtptatusAsync(int deviceId, string orderId, int orderStatus)
        {
            var response = new ResponseModel();
            try
            {
                var model = new UpdateOrderStatusModel()
                {
                    DeviceId = deviceId,
                    OrderId = orderId,
                    OrderStatus = orderStatus,
                };

                var reqData = JsonConvert.SerializeObject(model);
                response.MyRequestData = reqData;
                var content = new StringContent(reqData, Encoding.UTF8, "application/json");
                var url = "Withdraw/UpdateOtptatus";

                var result = await Client.PostAsync(url, content);
                response.MyRequestUri = result.RequestMessage.RequestUri;

                if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.MyIsSuccess = true;
                    var resStr = await result.Content.ReadAsStringAsync();
                    response = GetCommonResponse(response, resStr);
                    response.MyResponseString = resStr;
                }
            }
            catch (Exception ex)
            {
                response.MyErrorMessage = ex.Message;
                response.MyExceptionMessage = ex.ToString();
            }
            return response;
        }

        public async Task<ResponseModel> CreateOrderOtpAsync(int deviceId, string orderId, string transferOtp)
        {
            var response = new ResponseModel();
            try
            {
                var model = new CreateOrderOtpModel()
                {
                    DeviceId = deviceId,
                    OrderId = orderId,
                    TransferOtp = transferOtp,
                };

                var reqData = JsonConvert.SerializeObject(model);
                response.MyRequestData = reqData;
                var content = new StringContent(reqData, Encoding.UTF8, "application/json");
                var url = "Withdraw/CreateOrderOtp";

                var result = await Client.PostAsync(url, content);
                response.MyRequestUri = result.RequestMessage.RequestUri;

                if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.MyIsSuccess = true;
                    var resStr = await result.Content.ReadAsStringAsync();
                    response = GetCommonResponse(response, resStr);
                    response.MyResponseString = resStr;
                }
            }
            catch (Exception ex)
            {
                response.MyErrorMessage = ex.Message;
                response.MyExceptionMessage = ex.ToString();
            }
            return response;
        }

        public async Task<ResponseModel> GetSuccessOrderAsync(int deviceId, string orderId)
        {
            var response = new ResponseModel();
            try
            {
                var model = new GetSuccessOrderModel()
                {
                    DeviceId = deviceId,
                    OrderId = orderId,
                };

                var reqData = JsonConvert.SerializeObject(model);
                response.MyRequestData = reqData;
                var content = new StringContent(reqData, Encoding.UTF8, "application/json");
                var url = "Withdraw/GetSuccessOrder";

                var result = await Client.PostAsync(url, content);
                response.MyRequestUri = result.RequestMessage.RequestUri;

                if (result?.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.MyIsSuccess = true;
                    var resStr = await result.Content.ReadAsStringAsync();
                    response = GetCommonResponse(response, resStr);
                    response.MyResponseString = resStr;
                }
            }
            catch (Exception ex)
            {
                response.MyErrorMessage = ex.Message;
                response.MyExceptionMessage = ex.ToString();
            }
            return response;
        }

        #endregion


        #region Private

        private ResponseModel GetCommonResponse(ResponseModel myResponse, string responseString)
        {
            var response = JsonConvert.DeserializeObject<ResponseModel>(responseString);
            myResponse.data = response.data;
            myResponse.code = response.code;
            myResponse.message = response.message;
            myResponse.timestamp = response.timestamp;
            return myResponse;
        }

        #endregion
    }
}

using Neptune.NsPay.AcbBankScriptV2.Models;
using Neptune.NsPay.Commons;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Neptune.NsPay.AcbBankScriptV2
{
    public class HttpHelper
    {
        private readonly  static string ApiUrl = AppSettings.Configuration["ApiUrl"];

        /// <summary>
        /// 获取设备
        /// </summary>
        public static async Task<GetDeviceResponseData?> GetDeviceIdAsync()
        {
            try
            {
                var url = ApiUrl + "/Withdraw/GetDevice";
                var client = new RestClient(url);
                var paramData = new
                {
                    MerchantCode = AppSettings.Configuration["MerchantCode"],
                    Phone = AppSettings.Configuration["Phone"],
                    BankType = Convert.ToInt32(AppSettings.Configuration["BankType"])
                };
                var data = JsonConvert.SerializeObject(paramData);
                var request = new RestRequest()
                    .AddParameter("application/json", data, ParameterType.RequestBody);
                var response = await client.ExecutePostAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var detailResponse = JsonConvert.DeserializeObject<GetDeviceResponse>(response.Content);
                        if (detailResponse != null)
                        {
                            if (detailResponse.Code == 200 && detailResponse.Data != null)
                            {
                                return detailResponse.Data;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("获取设备异常:" + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 写入OTP
        /// </summary>
        public static async Task<bool> UpdateOrderOtpAsync(int deviceId,string otp)
        {
            try
            {
                var url = ApiUrl + "/Withdraw/UpdateOrderOtp";
                var client = new RestClient(url);
                var paramData = new
                {
                    DeviceId = deviceId,
                    Otp = otp,
                };
                var data = JsonConvert.SerializeObject(paramData);
                var request = new RestRequest()
                    .AddParameter("application/json", data, ParameterType.RequestBody);
                var response = await client.ExecutePostAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var detailResponse = JsonConvert.DeserializeObject<BaseResponse>(response.Content);
                        if (detailResponse != null)
                        {
                            if (detailResponse.Code == 200)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }catch (Exception ex)
            {
                NlogLogger.Error("上传otp错误:" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 获取otp
        /// </summary>
        public static async Task<GetOrderOtpResponseData?> GetOrderOtpAsync(int deviceId)
        {
            try
            {
                var url = ApiUrl + "/Withdraw/GetOrderOtp";
                var client = new RestClient(url);
                var paramData = new
                {
                    DeviceId = deviceId
                };
                var data = JsonConvert.SerializeObject(paramData);
                var request = new RestRequest()
                    .AddParameter("application/json", data, ParameterType.RequestBody);
                var response = await client.ExecutePostAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var detailResponse = JsonConvert.DeserializeObject<GetOrderOtpResponse>(response.Content);
                        if (detailResponse != null)
                        {
                            if (detailResponse.Code == 200)
                            {
                                return detailResponse.Data;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("获取缓存订单错误:" + ex.ToString());
                return null;
            }
        }

        public static async Task RemoveOrderOtpAsync(int deviceId, string orderId)
        {
            try
            {
                var url = ApiUrl + "/Withdraw/RemoveOrderOtp";
                var client = new RestClient(url);
                var paramData = new
                {
                    DeviceId = deviceId,
                    OrderId = orderId,
                };
                var data = JsonConvert.SerializeObject(paramData);
                var request = new RestRequest()
                    .AddParameter("application/json", data, ParameterType.RequestBody);
                await client.ExecutePostAsync(request);
            }
            catch (Exception ex)
            {
                NlogLogger.Error("上传otp错误:" + ex.ToString());
            }
        }
    }
}

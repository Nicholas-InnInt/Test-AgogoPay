using Abp.Extensions;
using Abp.Json;
using Amazon.Runtime.Internal;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.ScratchCard.Models;
using Neptune.NsPay.RedisExtensions.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.HttpExtensions.ScratchCard
{
    public class ScDoiTheCaoHelper: IScDoiTheCaoHelper
    {
        private readonly static string AddCardUrl = "https://congthe.biz/apis/card/add";
        private static readonly string CheckCarUrl = "https://congthe.biz/apis/card/result";
        
        public async Task<DoithecaoonlineResponse> AddCard(PayMentRedisModel payMent, ScratchCardRequest scratchCardRequest)
        {
            var response = await AddCard(AddCardUrl, payMent.CompanyKey, payMent.CompanySecret, scratchCardRequest);
            return response;
        }
        public async Task<DoithecaoonlineCheckCardResponse> CheckCard(PayMentRedisModel payMent, ScratchCardRequest scratchCardRequest)
        {
            var response = await CheckCard(CheckCarUrl, payMent.CompanyKey, payMent.CompanySecret, scratchCardRequest);
            return response;
        }

        private async Task<DoithecaoonlineResponse> AddCard(string url, string key, string secret, ScratchCardRequest scratchCardRequest)
        {
            try
            {
                var multipartFormDataContent = new MultipartFormDataContent
                {
                    { new StringContent(key), "client_token_api" },
                    { new StringContent(secret), "client_code_request" },
                    { new StringContent(MD5Helper.MD5Encrypt32(key + secret).ToLower()), "client_signature" },
                    { new StringContent(scratchCardRequest.TelcoName), "client_type_card" },
                    { new StringContent(scratchCardRequest.Code), "client_code_card" },
                    { new StringContent(scratchCardRequest.Seri), "client_seri_card" },
                    { new StringContent(scratchCardRequest.Amount.ToString("F0")), "client_value_card" },
                    { new StringContent(scratchCardRequest.Transactionid), "client_transaction_id" }
                };
                var response = await new HttpClient().PostAsync(url, multipartFormDataContent);
                var responseDetail = await response.Content.ReadAsStringAsync();
                if (!responseDetail.IsNullOrEmpty())
                {
                    var detailResponse = JsonConvert.DeserializeObject<DoithecaoonlineResponse>(responseDetail);
                    if(detailResponse == null)
                    {
                        return new DoithecaoonlineResponse() { success = false, code = 125, message = "系统异常" };
                    }
                    return detailResponse;
                }
                return new DoithecaoonlineResponse() { success = false, code = 125, message = "系统异常" };
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Doithecaoonline 添加card 异常：", ex);
                return new DoithecaoonlineResponse() { success = false, code = 125, message = "系统异常" };
            }
        }

        private async Task<DoithecaoonlineCheckCardResponse> CheckCard(string url, string key, string secret, ScratchCardRequest scratchCardRequest)
        {
            try
            {
                var multipartFormDataContent = new MultipartFormDataContent
                {
                    { new StringContent(key), "client_token_api" },
                    { new StringContent(secret), "client_code_request" },
                    { new StringContent(MD5Helper.MD5Encrypt32(key + secret).ToLower()), "client_signature" },
                    { new StringContent("trans"), "req_type" },
                    { new StringContent(scratchCardRequest.Transactionid), "req_value" }
                };
                var response = await new HttpClient().PostAsync(url, multipartFormDataContent);
                var responseDetail = await response.Content.ReadAsStringAsync();
                if (!responseDetail.IsNullOrEmpty())
                {
                    var detailResponse = JsonConvert.DeserializeObject<DoithecaoonlineCheckCardResponse>(responseDetail);
                    if (detailResponse == null)
                    {
                        return new DoithecaoonlineCheckCardResponse() { success = false, code = 125, message = "系统异常" };
                    }
                    return detailResponse;
                }
                return new DoithecaoonlineCheckCardResponse() { success = false, code = 125, message = "系统异常" };
            }
            catch (Exception ex)
            {
                NlogLogger.Error("Doithecaoonline 检查card 异常：", ex);
                return new DoithecaoonlineCheckCardResponse() { success = false, code = 125, message = "系统异常" };
            }
        }
    }
}

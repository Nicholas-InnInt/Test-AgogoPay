using Microsoft.Extensions.Configuration;
using Neptune.NsPay.BankApi.Models.TronCrypto;
using Neptune.NsPay.HttpExtensions.Crypto.Dtos;
using Neptune.NsPay.HttpExtensions.Crypto.Enums;
using Neptune.NsPay.HttpExtensions.Crypto.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Crypto.Models;
using RestSharp;

namespace Neptune.NsPay.HttpExtensions.Crypto.Helpers
{
    public class TronCryptoHelper : ITronCryptoHelper
    {
        private readonly IConfiguration _configuration;

        public TronCryptoHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<TokenTransactionDto>> GetTokenTransactionsByAddress(TronTokenTypeEnum tokenType, string address)
        {
            var baseUrl = _configuration.GetValue<string>("Crypto:Tron:ScannerUrl");

            var tokenId = tokenType switch
            {
                TronTokenTypeEnum.USDT => _configuration.GetValue<string>("Crypto:Tron:USDTTokenID"),
                _ => throw new ArgumentException("Unsupported token type", nameof(tokenType))
            };

            if (string.IsNullOrEmpty(baseUrl))
                throw new Exception("Tron ScannerUrl not found");

            var client = new RestClient($"{baseUrl}/api/transfer/trc20");
            var request = new RestRequest()
                    .AddParameter("address", address)
                    .AddParameter("trc20Id", tokenId)
                    .AddParameter("direction", (int)TronTransferDirectionEnum.Both)
                    .AddParameter("reverse", true)
                    .AddParameter("start_timestamp", "")
                    .AddParameter("end_timestamp", "");

            var response = await client.ExecuteGetAsync<TronAPIResponse<List<TronTokenDataResponse>>>(request);
            if (response.IsSuccessful && response.Data?.Data is { Count: > 0 } tokenDataList)
            {
                return tokenDataList.Select(data => new TokenTransactionDto
                {
                    TokenName = data.TokenName,
                    Amount = decimal.Parse(data.Amount) / (decimal)Math.Pow(10, data.Decimals),
                    From = data.From,
                    To = data.To,
                    Hash = data.Hash,
                    Status = data.Status,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(data.BlockTimestamp).UtcDateTime,
                }).ToList();
            }

            return null;
        }
    }
}
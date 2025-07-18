using Microsoft.Extensions.Configuration;
using Neptune.NsPay.HttpExtensions.Crypto.Dtos;
using Neptune.NsPay.HttpExtensions.Crypto.Enums;
using Neptune.NsPay.HttpExtensions.Crypto.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Crypto.Models;
using RestSharp;

namespace Neptune.NsPay.HttpExtensions.Crypto.Helpers
{
    public class EthereumCryptoHelper : IEthereumCryptoHelper
    {
        private readonly IConfiguration _configuration;

        public EthereumCryptoHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<TokenTransactionDto>> GetTokenTransactionsByAddress(TronTokenTypeEnum tokenType, string address)
        {
            var baseUrl = _configuration.GetValue<string>("Crypto:Ethereum:ScannerUrl");

            var tokenId = tokenType switch
            {
                TronTokenTypeEnum.USDT => _configuration.GetValue<string>("Crypto:Ethereum:USDTTokenID"),
                _ => throw new ArgumentException("Unsupported token type", nameof(tokenType))
            };

            if (string.IsNullOrEmpty(baseUrl))
                throw new Exception("Tron ScannerUrl not found");

            var apiKey = _configuration.GetValue<string>("Crypto:Ethereum:APIKey");
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Ethereum API Key not found");

            var client = new RestClient($"{baseUrl}/api");
            var request = new RestRequest()
                .AddParameter("module", "account")
                .AddParameter("action", "tokentx")
                .AddParameter("address", address)
                .AddParameter("contractaddress", tokenId)
                .AddParameter("offset", 20)
                .AddParameter("sort", "desc")
                .AddParameter("apikey", apiKey);

            var response = await client.ExecuteGetAsync<EthereumAPIResponse<List<EthereumTransactionDataResponse>>>(request);
            if (response.IsSuccessful && response.Data?.Result is { Count: > 0 } tokenDataList)
            {
                return tokenDataList.Select(data => new TokenTransactionDto
                {
                    TokenName = data.TokenName,
                    Amount = decimal.Parse(data.Value) / (decimal)Math.Pow(10, int.Parse(data.TokenDecimal)),
                    From = data.From,
                    To = data.To,
                    Hash = data.Hash,
                    Status = 1,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(data.TimeStamp)).UtcDateTime,
                }).ToList();
            }

            return null;
        }
    }
}
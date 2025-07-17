using Neptune.NsPay.HttpExtensions.Crypto.Models;
using System.Text.Json.Serialization;

namespace Neptune.NsPay.BankApi.Models.TronCrypto
{
    internal class TronAPIResponse<T>
    {
        public int Code { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        public int Total { get; set; }
        public int RangeTotal { get; set; }
        public T Data { get; set; }

        [JsonPropertyName("token_transfers")]
        public List<TronTokenTransferResponse> TokenTransfers { get; set; }

        public TronTokenInfoResponse TokenInfo { get; set; }
        public long WholeChainTxCount { get; set; }
        public Dictionary<string, bool> ContractMap { get; set; }
        public Dictionary<string, object> ContractInfo { get; set; }
        public Dictionary<string, TronNormalAddressInfoResponse> NormalAddressInfo { get; set; }
        public string From { get; set; }
        public int TimeInterval { get; set; }
    }
}
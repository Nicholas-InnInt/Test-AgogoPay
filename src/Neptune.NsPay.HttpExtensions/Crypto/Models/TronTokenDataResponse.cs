using Neptune.NsPay.HttpExtensions.Crypto.Enums;
using System.Text.Json.Serialization;

namespace Neptune.NsPay.HttpExtensions.Crypto.Models
{
    internal class TronTokenDataResponse
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("approval_amount")]
        public string ApprovalAmount { get; set; }

        [JsonPropertyName("block_timestamp")]
        public long BlockTimestamp { get; set; }

        [JsonPropertyName("block")]
        public int Block { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("confirmed")]
        public int Confirmed { get; set; }

        [JsonPropertyName("contract_type")]
        public string ContractType { get; set; }

        [JsonPropertyName("contractType")]
        public int ContractTypeCode { get; set; }

        [JsonPropertyName("revert")]
        public int Revert { get; set; }

        [JsonPropertyName("contract_ret")]
        public string ContractRet { get; set; }

        [JsonPropertyName("event_type")]
        public string EventType { get; set; }

        [JsonPropertyName("issue_address")]
        public string IssueAddress { get; set; }

        [JsonPropertyName("decimals")]
        public int Decimals { get; set; }

        [JsonPropertyName("token_name")]
        public string TokenName { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("direction")]
        public TronTransferDirectionEnum Direction { get; set; }
    }
}
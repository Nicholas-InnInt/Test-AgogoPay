using Neptune.NsPay.BankApi.Models.TronCrypto;

namespace Neptune.NsPay.HttpExtensions.Crypto.Models
{
    internal class TronTokenTransferResponse
    {
        public string TransactionId { get; set; }
        public int Status { get; set; }
        public long BlockTs { get; set; }
        public string FromAddress { get; set; }
        public Dictionary<string, object> FromAddressTag { get; set; }
        public string ToAddress { get; set; }
        public Dictionary<string, object> ToAddressTag { get; set; }
        public long Block { get; set; }
        public string ContractAddress { get; set; }
        public string Quant { get; set; }
        public bool Confirmed { get; set; }
        public string ContractRet { get; set; }
        public string FinalResult { get; set; }
        public bool Revert { get; set; }
        public TronTokenInfoResponse TokenInfo { get; set; }
        public string ContractType { get; set; }
        public bool FromAddressIsContract { get; set; }
        public bool ToAddressIsContract { get; set; }
        public bool RiskTransaction { get; set; }
    }
}

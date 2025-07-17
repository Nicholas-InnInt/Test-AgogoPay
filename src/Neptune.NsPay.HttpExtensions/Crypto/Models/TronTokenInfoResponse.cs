namespace Neptune.NsPay.BankApi.Models.TronCrypto
{
    internal class TronTokenInfoResponse
    {
        public string TokenId { get; set; }
        public string TokenAbbr { get; set; }
        public string TokenName { get; set; }
        public int TokenDecimal { get; set; }
        public int TokenCanShow { get; set; }
        public string TokenType { get; set; }
        public string TokenLogo { get; set; }
        public string TokenLevel { get; set; }
        public string IssuerAddr { get; set; }
        public bool Vip { get; set; }
    }
}
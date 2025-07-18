namespace Neptune.NsPay.HttpExtensions.Crypto.Dtos
{
    public class TokenTransactionDto
    {
        public string TokenName { get; set; }
        public decimal Amount { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Hash { get; set; }
        public int Status { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
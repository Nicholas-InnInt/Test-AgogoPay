using Neptune.NsPay.HttpExtensions.Crypto.Enums;

namespace Neptune.NsPay.HttpExtensions.Crypto.Dtos
{
    public class TronTokenTransactionDto
    {
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public long BlockTimestamp { get; set; }
        public int Block { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Hash { get; set; }
        public int Confirmed { get; set; }
        public string Id { get; set; }
        public TronTransferDirectionEnum Direction { get; set; }
    }
}
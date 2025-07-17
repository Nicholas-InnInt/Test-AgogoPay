namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class BankRecordModelInput
    {
        public string? MerchantCode { get; set; }
        public string Sign { get; set; }
        public int PayMentId { get; set; }
        public string Phone { get; set; }
        public string CardNumber { get; set; }
        public int Type { get; set; } = 0;
        public List<BankRecordModel> BankRecords { get; set; }
    }
    public class BankRecordModel
    {
        public string? TransactionTime { get; set; }
        public string? RefNo { get; set; }
        public string? Description { get; set; }
        public string? Amount { get; set; }
        public string? AvailableBalance { get; set; }
        public string? BankName { get; set; }
        public string? CreditAcctNo { get; set; }
        public string? CreditAcctName { get; set; }
        public string? DebitAcctNo { get; set; }
        public string? DebitAcctName { get; set; }
        public string? DorC { get; set; }
    }
}

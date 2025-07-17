using Neptune.NsPay.CefTransfer.Common.MyEnums;

namespace Neptune.NsPay.Transfer.Win.Models
{
    public class TransferInfoModel
    {
        // Config File
        //public int BankType { get; set; }
        //public string Phone { get; set; }

        // Local
        //public Bank Bank { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CardNo { get; set; }
        public string ToBank { get; set; }
        public string ToAccountNumber { get; set; }
        public string ToAccountName { get; set; }
        public decimal ToAmount { get; set; }
        public string ToRemarks { get; set; }
        public string OrderNo { get; set; }
        public string OrderId { get; set; }
        public int DeviceId { get; set; }

        // Result
        public decimal BalanceAfterTransfer { get; set; } = 0.00M;
        public TransferStatus Status { get; set; } = TransferStatus.Failed;
        public string TransactionId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

    }
}

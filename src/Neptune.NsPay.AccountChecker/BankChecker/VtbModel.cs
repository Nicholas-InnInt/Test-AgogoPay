namespace Neptune.NsPay.AccountChecker.BankChecker
{
    public class VtbGetExtBenModel
    {
        public string sessionId { get; set; }
        public VtbGetExtBenEncryptModel encrypt { get; set; }
    }

    public class VtbGetExtBenEncryptModel
    {
        public string fromAccount { get; set; }
        public string beneficiaryType { get; set; }
        public string beneficiaryAccount { get; set; }
        public string beneficiaryBin { get; set; }
    }

    public class VtbGetIntBenModel
    {
        public string sessionId { get; set; }
        public VtbGetIntBenEncryptModel encrypt { get; set; }
    }

    public class VtbGetIntBenEncryptModel
    {
        public string accountNumber { get; set; }
    }

    public class VtbCommonEncryptedRequestModel
    {
        public string encrypted { get; set; }
    }
    public class VtbBankResponseModel
    {
        public string requestId { get; set; }
        public string sessionId { get; set; }
        public bool error { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string systemDate { get; set; }
        public string beneficiaryName { get; set; }
        public string beneficiaryBank { get; set; }
        public string toAccountName { get; set; }
    }

}

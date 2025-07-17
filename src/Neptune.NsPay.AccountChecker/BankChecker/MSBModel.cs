namespace Neptune.NsPay.AccountChecker.BankChecker
{

    public class MsbGetIntBenModel
    {
        public string toBenefitAcc { get; set; }
        public string tokenNo { get; set; }
        public string lang { get; set; }
    }

    public class MsbGetExtBenModel
    {
        public string bankCode { get; set; }
        public string beneficiaryAccount { get; set; }
        public string type { get; set; }
        public string fromAcct { get; set; }
        public string tokenNo { get; set; }
        public string lang { get; set; }
    }

    public class MsbExtBenResponseModel
    {
        public string status { get; set; }
        public string message { get; set; }
        public string inforError { get; set; }
        public object tokenNo { get; set; }
        public object timeCreateToken { get; set; }
        public MSBExtBankInfo data { get; set; }
        public object peekData { get; set; }

    }

    public class MSBExtBankInfo
    {
        public int benefId { get; set; }
        public int userId { get; set; }
        public object isInterBank { get; set; }
        public object beneficiaryAccountNo { get; set; }
        public string beneficiaryName { get; set; }
    }


    public class MsIntBenResponseModel
    {
        public string status { get; set; }
        public string message { get; set; }
        public string inforError { get; set; }
        public object tokenNo { get; set; }
        public object timeCreateToken { get; set; }
        public MSBIntBankInfo data { get; set; }
        public object peekData { get; set; }
    }

    public class MSBIntBankInfo
    {
        public string name { get; set; }

    }

}

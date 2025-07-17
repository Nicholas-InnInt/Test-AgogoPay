namespace Neptune.NsPay.AccountChecker.BankChecker
{
    public class BidvDecryptResponseModel
    {
        public object userJwt { get; set; }
        public string d { get; set; }
        public string k { get; set; }
        public object file { get; set; }
    }
    public class BidvEncryptedRequestModel
    {
        public string x_request_id { get; set; }
        public string token { get; set; } // Not in use
        public BidvEncryptedRequestBodyModel body { get; set; }
    }

    public class BidvEncryptedRequestBodyModel
    {
        public string d { get; set; }
        public string k { get; set; }
    }
    public class BidvExtBeneficiaryModel
    {
        //public int mid { get; set; }
        //public string code { get; set; }
        //public string des { get; set; }
        //public string requestId { get; set; }
        //public bool showDes { get; set; }
        //public object[] messages { get; set; }
        public string beneName { get; set; }
        //public string bankName { get; set; }
        //public string urlLogo { get; set; }
        //public string bankId { get; set; }
        //public string sortName { get; set; }
        //public string receiverBank { get; set; }
        //public string receiverName { get; set; }
        //public string interbankRef { get; set; }
        //public bool success { get; set; }
        //public string clientPubKey { get; set; }
        //public string user { get; set; }
        //public string clientIP { get; set; }
    }


    public class BidvGetExtBenModel
    {
        public string browserId { get; set; }
        public string clientId { get; set; }
        public string appVersionId { get; set; }
        public BidvGetExtBenEncryptModel encrypt { get; set; }
    }

    public class BidvGetExtBenEncryptModel
    {
        public string accNo { get; set; }
        public string bankCode247 { get; set; }
    }

    public class BidvGetIntBenModel
    {
        public string browserId { get; set; }
        public string clientId { get; set; }
        public string appVersionId { get; set; }
        public BidvGetIntBenEncryptModel encrypt { get; set; }
    }

    public class BidvGetIntBenEncryptModel
    {
        public string accNo { get; set; }
    }


    public class BidvIntBeneficiaryModel
    {
        //public int mid { get; set; }
        //public string code { get; set; }
        //public string requestId { get; set; }
        //public bool showDes { get; set; }
        //public object[] messages { get; set; }
        //public string accNo { get; set; }
        public string accName { get; set; }
        //public string cifNo { get; set; }
        //public string accNoR { get; set; }
        //public string branchCode { get; set; }
        //public string oldAccNo { get; set; }
        //public bool success { get; set; }
        //public string des { get; set; }
        //public string clientPubKey { get; set; }
        //public string user { get; set; }
        //public string clientIP { get; set; }
    }
}

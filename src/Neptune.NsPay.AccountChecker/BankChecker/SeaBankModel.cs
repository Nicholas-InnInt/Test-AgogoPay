namespace Neptune.NsPay.AccountChecker.BankChecker
{

    public class SeaGetExtBenModel
    {
        public string benAccount { get; set; }
        public string bankID { get; set; }
        public string senderAccount { get; set; }
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class SeaAccountInfo
    {
        public string accountID { get; set; }
        public string bankID { get; set; }
        public string accountName { get; set; }
    }

    public class SeaResponseContentModel
    {
        public object errorCode { get; set; }
        public object errorMessage { get; set; }
        public string mti { get; set; }
        public string processingCode { get; set; }
        public string tranAmount { get; set; }
        public string transmissionDateTime { get; set; }
        public string traceAuditNumber { get; set; }
        public string localTranTime { get; set; }
        public string localTranDate { get; set; }
        public object settlementDate { get; set; }
        public string merchantType { get; set; }
        public object accquiCountry { get; set; }
        public object posEntryCondCode { get; set; }
        public string acceptInsIdentCode { get; set; }
        public string referenceNumber { get; set; }
        public string authIdentResp { get; set; }
        public string responseCode { get; set; }
        public string cardAcceptorId { get; set; }
        public object cardAcceptorCode { get; set; }
        public string debitCcy { get; set; }
        public string bankID { get; set; }
        public string senderAccount { get; set; }
        public string recInstIdCode { get; set; }
        public SeaAccountInfo accountInfo { get; set; }
        public string serviceCode { get; set; }
        public string additionalDataPrivate { get; set; }
        public string fromAccIdentification { get; set; }
        public string mac { get; set; }
    }

    public class SeaResponseModel
    {
        public string code { get; set; }
        public string message { get; set; }
        public object messageVi { get; set; }
        public object messageEn { get; set; }
        public SeaResponseContentModel data { get; set; }
    }


    public class SeaIntResponseModel
    {
        public string code { get; set; }
        public string message { get; set; }
        public string messageVi { get; set; }
        public string messageEn { get; set; }
        public SeaIntResponseData data { get; set; }
    }

    public class SeaIntResponseData
    {
        public string errorCode { get; set; }
        public object errorMessage { get; set; }
        public string responseCode { get; set; }
        public List<SeaIntAccount> account { get; set; }
    }

   public class SeaIntAccount
    {

            public string accountType { get; set; }
            public string shortName { get; set; }
            public string shortTitle { get; set; }
            public string currency { get; set; }
    }

}

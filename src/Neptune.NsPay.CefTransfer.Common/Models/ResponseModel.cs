namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class ResponseModel
    {
        // Api Response
        public object data { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public string timestamp { get; set; }

        // My Response
        public bool MyIsSuccess { get; set; }
        public string MyErrorMessage { get; set; }
        public string MyExceptionMessage { get; set; }
        public string MyResponseString { get; set; }
        public string MyRequestData { get; set; }
        public Uri MyRequestUri { get; set; }
    }
}

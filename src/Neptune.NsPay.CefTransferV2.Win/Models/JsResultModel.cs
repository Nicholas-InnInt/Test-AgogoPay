namespace Neptune.NsPay.CefTransferV2.Win.Models
{
    public class JsResultModel
    {
        public bool BoolResult { get; set; }
        public string? StringResult { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuccess { get; set; } // Is Js Executed without exception
    }
}

namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class VcbReportModel
    {
        public string TransferType { get; set; }
        public bool Result { get; set; }
        public int LoadBank { get; set; }
        public int IsLoggedIn { get; set; }
        public int GetLang { get; set; }
        public int Login { get; set; }
        public int GoTransfer { get; set; }
        public int InputTransfer1 { get; set; }
        public int InputTransfer2 { get; set; }
        public int InputTransfer3 { get; set; }
        public int TransferStatus { get; set; }
    }
}

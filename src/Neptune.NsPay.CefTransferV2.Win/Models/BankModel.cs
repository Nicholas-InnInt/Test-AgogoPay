using Neptune.NsPay.CefTransfer.Common.MyEnums;

namespace Neptune.NsPay.CefTransferV2.Win.Models
{
    public class BankModel
    {
        public Bank id { get; set; }
        public string name { get; set; }
        public string domain { get; set; }
        public string url { get; set; }
        public string ele_user { get; set; }
        public string ele_password { get; set; }
        public string ele_corp { get; set; }
    }
}
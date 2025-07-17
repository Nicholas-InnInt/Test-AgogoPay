using System.Text.RegularExpressions;

namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class MsbReportModel
    {
        public string TransferType { get; set; }
        public bool Result { get; set; }
        public int GoHome { get; set; }
        public int IsLoggedIn { get; set; }
        public int GetLang { get; set; }
        public int InputLogin { get; set; }
        public int ManualLogin { get; set; }    
        public int IsLoggedIn2 { get; set; }
        public int GoTransfer { get; set; }
        public int InputTransfer { get; set; }
        public int TransferAuth { get; set; }
        public int TransferStatus { get; set; }
        public string Statistic
        {
            get
            {
                return Regex.Replace(
                    @$"
                        {TransferType}|
                        {GoHome}|
                        {IsLoggedIn}|
                        {GetLang}|
                        {InputLogin}|
                        {ManualLogin}|
                        {IsLoggedIn2}|
                        {GoTransfer}|
                        {InputTransfer}|
                        {TransferAuth}|
                        {TransferStatus}
                    ", @"\s+", string.Empty);
            }
        }
    }
}

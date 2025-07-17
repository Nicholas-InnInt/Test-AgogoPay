using System.Text.RegularExpressions;

namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class VtbReportModel
    {
        public string TransferType { get; set; }
        public bool Result { get; set; }
        public int LoadBank { get; set; }
        public int IsLoggedIn { get; set; }
        public int Reload { get; set; }
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
                        {LoadBank}|
                        {IsLoggedIn}|
                        {Reload}|
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


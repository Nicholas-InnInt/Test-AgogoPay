using System.Text.RegularExpressions;

namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class AcbReportModel
    {
        public string TransferType { get; set; }
        public bool Result { get; set; }
        public int IsLoggedIn { get; set; }
        public int GoDashboard { get; set; }
        public int Reload { get; set; }
        public int GetLang { get; set; }
        public int InputLogin { get; set; }
        public int ManualLogin { get; set; }
        public int LoginAuth { get; set; }
        public int IsLoggedIn2 { get; set; }
        public int GoTransfer { get; set; }
        public int InputTransfer { get; set; }
        public int ConfirmTransfer { get; set; }
        public int TransferStatus { get; set; }
        public string Statistic
        {
            get
            {
                return Regex.Replace(
                    @$"
                        {TransferType}|
                        {IsLoggedIn}|
                        {GoDashboard}|
                        {Reload}|
                        {GetLang}|
                        {InputLogin}|
                        {ManualLogin}|
                        {LoginAuth}|
                        {IsLoggedIn2}|
                        {GoTransfer}|
                        {InputTransfer}|
                        {ConfirmTransfer}|
                        {TransferStatus}
                    ", @"\s+", string.Empty);
            }
        }
    }
}

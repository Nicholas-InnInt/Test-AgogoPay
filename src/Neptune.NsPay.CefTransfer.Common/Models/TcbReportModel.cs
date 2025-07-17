using System.Text.RegularExpressions;

namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class TcbReportModel
    {
        public string TransferType { get; set; }
        public bool Result { get; set; }
        public int LoadBank { get; set; }
        public int IsLoggedIn { get; set; }
        public int GetLang { get; set; }
        public int Login { get; set; }
        public int LoginAuth { get; set; }
        public int GoTransfer { get; set; }
        public int InputTransfer { get; set; }
        public int ConfirmTransfer { get; set; }
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
                        {GetLang}|
                        {Login}|
                        {LoginAuth}|
                        {GoTransfer}|
                        {InputTransfer}|
                        {ConfirmTransfer}|
                        {TransferAuth}|
                        {TransferStatus}
                    ", @"\s+", string.Empty);
            }
        }
    }
}
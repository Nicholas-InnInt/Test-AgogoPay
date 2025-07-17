namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class VcbVerificationCodeModel
    {
        public string Url { get; set; }
        public string CaptchatId { get; set; }
        public byte[] CaptchaImage { get; set; }
    }
}

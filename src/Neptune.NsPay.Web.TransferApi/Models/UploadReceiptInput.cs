namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class UploadReceiptInput
    {
        public string OrderId { get; set; }
        public string FileContentBase64 { get; set; }
        public string? fileExtension { get; set; }
    }
}

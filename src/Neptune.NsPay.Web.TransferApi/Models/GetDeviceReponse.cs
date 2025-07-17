namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class GetDeviceReponse
    {
        public int DeviceId { get;set; }
        public string Phone { get; set; }
        public string CardNumber { get; set; }
        public string Name { get; set; }
        public string Otp { get; set; }
        public bool Status { get; set;}
        public int Process { get; set; }
        public string LoginPassWord { get; set; }
    }
}

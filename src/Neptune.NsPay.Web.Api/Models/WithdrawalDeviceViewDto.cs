namespace Neptune.NsPay.Web.Api.Models
{
    public class WithdrawalDeviceViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public bool Status { get; set; }
        public int Process { get; set; }

        public string LoginPassWord { get; set; }
        public List<string> BankOtp { get; set; }

    }
}

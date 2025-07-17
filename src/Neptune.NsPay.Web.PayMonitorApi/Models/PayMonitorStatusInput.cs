namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class PayMonitorStatusInput:BaseInput
    {
        public int PayMentId { get; set; }
        public int Status { get; set; }
        public string Account { get; set; }

        public string MerchantCode { get; set; }
    }
}

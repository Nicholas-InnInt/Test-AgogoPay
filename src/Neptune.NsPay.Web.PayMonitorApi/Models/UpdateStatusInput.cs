namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class UpdateStatusInput
    {
        public int PayMentId { get; set; }
        public int Status { get; set; }
        public string Account { get; set; }
    }
}

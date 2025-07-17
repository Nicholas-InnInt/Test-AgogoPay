using Neptune.NsPay.Dto;

namespace Neptune.NsPay.WebHooks.Dto
{
    public class GetAllSendAttemptsInput : PagedInputDto
    {
        public string SubscriptionId { get; set; }
    }
}

using Abp.Application.Services.Dto;
using Abp.Webhooks;
using Neptune.NsPay.WebHooks.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Webhooks
{
    public class CreateOrEditWebhookSubscriptionViewModel
    {
        public WebhookSubscription WebhookSubscription { get; set; }

        public ListResultDto<GetAllAvailableWebhooksOutput> AvailableWebhookEvents { get; set; }
    }
}

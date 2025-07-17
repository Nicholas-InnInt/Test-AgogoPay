using System.Threading.Tasks;
using Abp.Webhooks;

namespace Neptune.NsPay.WebHooks
{
    public interface IWebhookEventAppService
    {
        Task<WebhookEvent> Get(string id);
    }
}

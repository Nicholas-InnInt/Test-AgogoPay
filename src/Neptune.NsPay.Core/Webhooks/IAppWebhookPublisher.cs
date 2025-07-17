using System.Threading.Tasks;
using Neptune.NsPay.Authorization.Users;

namespace Neptune.NsPay.WebHooks
{
    public interface IAppWebhookPublisher
    {
        Task PublishTestWebhook();
    }
}

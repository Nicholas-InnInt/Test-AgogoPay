using Microsoft.AspNetCore.SignalR;
using Neptune.NsPay.Web.PayMonitorApi.Models;
using Neptune.NsPay.Web.PayMonitorApi.Models.SignalR;

namespace Neptune.NsPay.Web.PayMonitorApi.Helpers
{
    public interface INotificationHubClient
    {
        Task SendMessage(string user, string message);
        Task UpdateBalance(BalanceUpdateNotification input);

        [HubMethodName("MerchantPaymentChanged")]
        Task MerchantPaymentChanged(MerchantPaymentDetails input);
    }
}

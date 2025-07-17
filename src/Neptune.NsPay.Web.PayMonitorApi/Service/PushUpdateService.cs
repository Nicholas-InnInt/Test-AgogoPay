using Microsoft.AspNetCore.SignalR;
using Neptune.NsPay.Web.PayMonitorApi.Helpers;
using Neptune.NsPay.Web.PayMonitorApi.Models;
using Neptune.NsPay.Web.PayMonitorApi.Models.SignalR;

namespace Neptune.NsPay.Web.PayMonitorApi.Service
{
    public class PushUpdateService : IPushUpdateService
    {
        private readonly IHubContext<NotificationHub, INotificationHubClient> _hub;

        public PushUpdateService(IHubContext<NotificationHub, INotificationHubClient> hub)
        {
            _hub = hub;
        }

        public async Task<bool> BalanceChanged(BalanceUpdateNotification input)
        {
            await _hub.Clients.All.UpdateBalance(input);
            return true;
        }

        public async Task<bool> MerchantPaymentChanged(MerchantPaymentDetails input)
        {
            await _hub.Clients.Group(input.MerchantCode).MerchantPaymentChanged(input);
            return true;
        }
    }
}

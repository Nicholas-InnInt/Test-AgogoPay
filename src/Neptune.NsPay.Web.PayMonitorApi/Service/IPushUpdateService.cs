using Neptune.NsPay.Web.PayMonitorApi.Models;
using Neptune.NsPay.Web.PayMonitorApi.Models.SignalR;

namespace Neptune.NsPay.Web.PayMonitorApi.Service
{
    public interface IPushUpdateService
    {
        Task<bool> BalanceChanged(BalanceUpdateNotification input);

        Task<bool> MerchantPaymentChanged(MerchantPaymentDetails input);
    }
}

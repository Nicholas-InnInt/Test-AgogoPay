using System.Threading.Tasks;
using Abp.Application.Services;
using Neptune.NsPay.MultiTenancy.Payments.PayPal.Dto;

namespace Neptune.NsPay.MultiTenancy.Payments.PayPal
{
    public interface IPayPalPaymentAppService : IApplicationService
    {
        Task ConfirmPayment(long paymentId, string paypalOrderId);

        PayPalConfigurationDto GetConfiguration();
    }
}

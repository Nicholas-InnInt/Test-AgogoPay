using System.Threading.Tasks;
using Abp.Application.Services;
using Neptune.NsPay.MultiTenancy.Payments.Dto;
using Neptune.NsPay.MultiTenancy.Payments.Stripe.Dto;

namespace Neptune.NsPay.MultiTenancy.Payments.Stripe
{
    public interface IStripePaymentAppService : IApplicationService
    {
        Task ConfirmPayment(StripeConfirmPaymentInput input);

        StripeConfigurationDto GetConfiguration();
        
        Task<string> CreatePaymentSession(StripeCreatePaymentSessionInput input);
    }
}
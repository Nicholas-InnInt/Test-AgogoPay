using Abp.Dependency;
using Abp.Extensions;
using Abp.Runtime.Session;
using Neptune.NsPay.Editions;
using Neptune.NsPay.ExtraProperties;
using Neptune.NsPay.MultiTenancy.Payments;
using Neptune.NsPay.Url;

namespace Neptune.NsPay.Web.Url
{
    public class PaymentUrlGenerator : IPaymentUrlGenerator, ITransientDependency
    {
        private readonly IWebUrlService _webUrlService;

        public PaymentUrlGenerator(
            IWebUrlService webUrlService)
        {
            _webUrlService = webUrlService;
        }

        public string CreatePaymentRequestUrl(SubscriptionPayment subscriptionPayment)
        {
            var webSiteRootAddress = _webUrlService.GetSiteRootAddress();

            var url = webSiteRootAddress.EnsureEndsWith('/') +
                      "Payment/GatewaySelection" +
                      "?paymentId=" + subscriptionPayment.Id;

            return url;
        }
    }
}
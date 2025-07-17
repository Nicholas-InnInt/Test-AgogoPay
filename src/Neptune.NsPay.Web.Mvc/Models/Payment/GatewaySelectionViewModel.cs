using System.Collections.Generic;
using System.Linq;
using Neptune.NsPay.MultiTenancy.Payments;
using Neptune.NsPay.MultiTenancy.Payments.Dto;

namespace Neptune.NsPay.Web.Models.Payment
{
    public class GatewaySelectionViewModel
    {
        public SubscriptionPaymentDto Payment { get; set; }
        
        public List<PaymentGatewayModel> PaymentGateways { get; set; }

        public bool AllowRecurringPaymentOption()
        {
            return Payment.AllowRecurringPayment() && PaymentGateways.Any(gateway => gateway.SupportsRecurringPayments);
        }
    }
}

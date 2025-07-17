using Neptune.NsPay.Editions;
using Neptune.NsPay.Editions.Dto;
using Neptune.NsPay.MultiTenancy.Payments;
using Neptune.NsPay.Security;
using Neptune.NsPay.MultiTenancy.Payments.Dto;

namespace Neptune.NsPay.Web.Models.TenantRegistration
{
    public class TenantRegisterViewModel
    {
        public int? EditionId { get; set; }

        public EditionSelectDto Edition { get; set; }
        
        public PasswordComplexitySetting PasswordComplexitySetting { get; set; }

        public EditionPaymentType EditionPaymentType { get; set; }
        
        public SubscriptionStartType? SubscriptionStartType { get; set; }
        
        public PaymentPeriodType? PaymentPeriodType { get; set; }
        
        public string SuccessUrl { get; set; }
        
        public string ErrorUrl { get; set; }
    }
}

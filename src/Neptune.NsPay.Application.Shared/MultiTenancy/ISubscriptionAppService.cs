using System.Threading.Tasks;
using Abp.Application.Services;
using Neptune.NsPay.MultiTenancy.Dto;
using Neptune.NsPay.MultiTenancy.Payments.Dto;

namespace Neptune.NsPay.MultiTenancy
{
    public interface ISubscriptionAppService : IApplicationService
    {
        Task DisableRecurringPayments();

        Task EnableRecurringPayments();
        
        Task<long> StartExtendSubscription(StartExtendSubscriptionInput input);
        
        Task<StartUpgradeSubscriptionOutput> StartUpgradeSubscription(StartUpgradeSubscriptionInput input);
        
        Task<long> StartTrialToBuySubscription(StartTrialToBuySubscriptionInput input);
    }
}

using Abp.Events.Bus;

namespace Neptune.NsPay.MultiTenancy.Subscription
{
    public class RecurringPaymentsEnabledEventData : EventData
    {
        public int TenantId { get; set; }
    }
}
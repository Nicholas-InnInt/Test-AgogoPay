using Neptune.NsPay.MultiTenancy.Dto;
using Neptune.NsPay.Sessions.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Editions
{
    public class SubscriptionDashboardViewModel
    {
        public GetCurrentLoginInformationsOutput LoginInformations { get; set; }
        
        public EditionsSelectOutput Editions { get; set; }
    }
}

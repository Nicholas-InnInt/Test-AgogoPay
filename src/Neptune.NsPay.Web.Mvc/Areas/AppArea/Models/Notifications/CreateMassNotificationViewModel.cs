using System.Collections.Generic;
using Abp.Notifications;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Notifications
{
    public class CreateMassNotificationViewModel
    {
        public List<string> TargetNotifiers { get; set; }
    
        public NotificationSeverity Severity { get; set; }
    }
}
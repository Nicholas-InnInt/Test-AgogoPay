using System;
using Abp.Notifications;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.Notifications.Dto
{
    public class GetUserNotificationsInput : PagedInputDto
    {
        public UserNotificationState? State { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
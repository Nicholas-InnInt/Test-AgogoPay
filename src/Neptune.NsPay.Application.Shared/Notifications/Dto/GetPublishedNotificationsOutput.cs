﻿using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Notifications;

namespace Neptune.NsPay.Notifications.Dto
{
    public class GetPublishedNotificationsOutput : PagedResultDto<GetNotificationsCreatedByUserOutput>
    {
        public GetPublishedNotificationsOutput(
            List<GetNotificationsCreatedByUserOutput> notificationsCreatedByUserOutput)
            : base(notificationsCreatedByUserOutput.Count, notificationsCreatedByUserOutput)
        {
        }
    }
}
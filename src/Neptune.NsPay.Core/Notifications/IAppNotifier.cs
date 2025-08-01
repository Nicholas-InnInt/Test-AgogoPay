﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.Localization;
using Abp.Notifications;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.MultiTenancy;

namespace Neptune.NsPay.Notifications
{
    public interface IAppNotifier
    {
        Task WelcomeToTheApplicationAsync(User user);

        Task NewUserRegisteredAsync(User user);

        Task NewTenantRegisteredAsync(Tenant tenant);

        Task GdprDataPrepared(UserIdentifier user, Guid binaryObjectId);

        Task SendMessageAsync(UserIdentifier user, string message, NotificationSeverity severity = NotificationSeverity.Info);

        Task SendMessageAsync(string notificationName, string message, UserIdentifier[] userIds = null,
            NotificationSeverity severity = NotificationSeverity.Info);
        
        Task SendMessageAsync(UserIdentifier user, LocalizableString localizableMessage, IDictionary<string, object> localizableMessageData = null, NotificationSeverity severity = NotificationSeverity.Info);

        Task TenantsMovedToEdition(UserIdentifier user, string sourceEditionName, string targetEditionName);

        Task SomeUsersCouldntBeImported(UserIdentifier user, string fileToken, string fileType, string fileName);

        Task SendMassNotificationAsync(string message, UserIdentifier[] userIds = null, 
            NotificationSeverity severity = NotificationSeverity.Info, Type[] targetNotifiers = null);

        Task ExporterPayOrderAsync(UserIdentifier user, string fileToken, string fileType, string fileName);
        Task ExporterPayDepositAsync(UserIdentifier user, string fileToken, string fileType, string fileName);
        Task ExporterMerchantBillsAsync(UserIdentifier user, string fileToken, string fileType, string fileName);

        Task ExporterWithdrawalOrderAsync(UserIdentifier user, string fileToken, string fileType, string fileName);
    }
}

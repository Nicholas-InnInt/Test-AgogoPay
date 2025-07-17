using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Localization;
using Abp.MultiTenancy;

namespace Neptune.NsPay.Authorization
{
    /// <summary>
    /// Application's authorization provider.
    /// Defines permissions for the application.
    /// See <see cref="AppPermissions"/> for all permission names.
    /// </summary>
    public class AppAuthorizationProvider : AuthorizationProvider
    {
        private readonly bool _isMultiTenancyEnabled;

        public AppAuthorizationProvider(bool isMultiTenancyEnabled)
        {
            _isMultiTenancyEnabled = isMultiTenancyEnabled;
        }

        public AppAuthorizationProvider(IMultiTenancyConfig multiTenancyConfig)
        {
            _isMultiTenancyEnabled = multiTenancyConfig.IsEnabled;
        }

        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)

            var napaypages = context.GetPermissionOrNull(AppPermissions.NsPayPages) ?? context.CreatePermission(AppPermissions.NsPayPages, L("NsPayPages"));

            #region 提现管理

            var WithdrawManager = napaypages.CreateChildPermission(AppPermissions.Pages_WithdrawManagers, L("WithdrawManager"));

            var MerchantDashboard = WithdrawManager.CreateChildPermission(AppPermissions.Pages_MerchantDashboard, L("MerchantDashboard"));

            var merchantWithdraws = WithdrawManager.CreateChildPermission(AppPermissions.Pages_MerchantWithdraws, L("MerchantWithdraws"));
            merchantWithdraws.CreateChildPermission(AppPermissions.Pages_MerchantWithdraws_Create, L("Create"));
            merchantWithdraws.CreateChildPermission(AppPermissions.Pages_MerchantWithdraws_Edit, L("Edit"));
            merchantWithdraws.CreateChildPermission(AppPermissions.Pages_MerchantWithdraws_Delete, L("Delete"));
            merchantWithdraws.CreateChildPermission(AppPermissions.Pages_MerchantWithdraws_AuditPass, L("PassMerchantWithdraw"));
            merchantWithdraws.CreateChildPermission(AppPermissions.Pages_MerchantWithdraws_AuditTurndown, L("TurndownMerchantWithdraw"));

            var merchantWithdrawBanks = WithdrawManager.CreateChildPermission(AppPermissions.Pages_MerchantWithdrawBanks, L("MerchantWithdrawBanks"));
            merchantWithdrawBanks.CreateChildPermission(AppPermissions.Pages_MerchantWithdrawBanks_Create, L("Create"));
            merchantWithdrawBanks.CreateChildPermission(AppPermissions.Pages_MerchantWithdrawBanks_Edit, L("Edit"));
            merchantWithdrawBanks.CreateChildPermission(AppPermissions.Pages_MerchantWithdrawBanks_Delete, L("Delete"));

            var withdrawalOrders = WithdrawManager.CreateChildPermission(AppPermissions.Pages_WithdrawalOrders, L("WithdrawalOrders"));
            //withdrawalOrders.CreateChildPermission(AppPermissions.Pages_WithdrawalOrders_Edit, L("EditWithdrawalOrder"));
            withdrawalOrders.CreateChildPermission(AppPermissions.Pages_WithdrawalOrders_CallBack, L("CallBackWithdrawalOrder"));
            withdrawalOrders.CreateChildPermission(AppPermissions.Pages_WithdrawalOrders_View, L("View"));
            withdrawalOrders.CreateChildPermission(AppPermissions.Pages_WithdrawalOrders_Cancel, L("CancelWithdrawalOrder"));
            withdrawalOrders.CreateChildPermission(AppPermissions.Pages_WithdrawalOrders_ChangeDevice, L("WithdrawalOrderChangeDevice"));
            withdrawalOrders.CreateChildPermission(AppPermissions.Pages_WithdrawalOrders_ViewPayoutDetails, L("WithdrawalOrderViewPayoutDetails"));
            withdrawalOrders.CreateChildPermission(AppPermissions.Pages_WithdrawalOrders_ChangeToPendingStatus, L("WithdrawalOrderChangeToPendingStatus"));
            withdrawalOrders.CreateChildPermission(AppPermissions.Pages_WithdrawalOrders_ReleaseBalance, L("WithdrawalOrderReleaseBalance"));


            var withdrawalDevices = WithdrawManager.CreateChildPermission(AppPermissions.Pages_WithdrawalDevices, L("WithdrawalDevices"));
            withdrawalDevices.CreateChildPermission(AppPermissions.Pages_WithdrawalDevices_Create, L("Create"));
            withdrawalDevices.CreateChildPermission(AppPermissions.Pages_WithdrawalDevices_Edit, L("Edit"));
            withdrawalDevices.CreateChildPermission(AppPermissions.Pages_WithdrawalDevices_Delete, L("Delete"));
            withdrawalDevices.CreateChildPermission(AppPermissions.Pages_WithdrawalDevices_ChildEdit, L("ChildEdit"));
            withdrawalDevices.CreateChildPermission(AppPermissions.Pages_WithdrawalDevices_BatchPauseBank, L("BatchPauseWithdrawalDevice"));

            #endregion

            #region 订单管理

            var payOrderManage = napaypages.CreateChildPermission(AppPermissions.Pages_PayOrderManagers, L("PayOrderManager"));

            var payOrders = payOrderManage.CreateChildPermission(AppPermissions.Pages_PayOrders, L("PayOrders"));
            //payOrders.CreateChildPermission(AppPermissions.Pages_PayOrders_Edit, L("EditPayOrder"));
            payOrders.CreateChildPermission(AppPermissions.Pages_PayOrders_CallBcak, L("CallBackPayOrder"));
            payOrders.CreateChildPermission(AppPermissions.Pages_PayOrders_EnforceCallBcak, L("EnforceCallBcakPayOrder"));
            payOrders.CreateChildPermission(AppPermissions.Pages_PayOrders_AddMerchantBill, L("AddMerchantBill"));
            //payOrders.CreateChildPermission(AppPermissions.Pages_PayOrders_ImmediateCallBcak, L("ImmediateCallBcakPayOrder"));

            var viewPayOrders = payOrders.CreateChildPermission(AppPermissions.Pages_PayOrders_View, L("ViewPayOrder"));
            viewPayOrders.CreateChildPermission(AppPermissions.Pages_PayOrders_View_BankInfo, L("ViewBankInfo"));

            var payOrderDeposits = payOrderManage.CreateChildPermission(AppPermissions.Pages_PayOrderDeposits, L("PayOrderDeposits"));
            payOrderDeposits.CreateChildPermission(AppPermissions.Pages_PayOrderDeposits_Edit, L("EditPayOrderDeposit"));

            var bankBalance = payOrderManage.CreateChildPermission(AppPermissions.Pages_BankBalances, L("BankBalances"));
            bankBalance.CreateChildPermission(AppPermissions.Pages_BankBalances_View, L("ViewBankBalances"));

            var rechargeOrders = payOrderManage.CreateChildPermission(AppPermissions.Pages_RechargeOrders, L("RechargeOrders"));
            rechargeOrders.CreateChildPermission(AppPermissions.Pages_RechargeOrders_Create, L("CreateRechargeOrders"));

            #endregion

            #region ns系统管理

            var nsPayManage = napaypages.CreateChildPermission(AppPermissions.Pages_NsPayManagers, L("NsPayManagers"));

            var nsPayBackgroundJobs = nsPayManage.CreateChildPermission(AppPermissions.Pages_NsPayBackgroundJobs, L("NsPayBackgroundJobs"));
            nsPayBackgroundJobs.CreateChildPermission(AppPermissions.Pages_NsPayBackgroundJobs_Create, L("Create"));
            nsPayBackgroundJobs.CreateChildPermission(AppPermissions.Pages_NsPayBackgroundJobs_Edit, L("Edit"));
            nsPayBackgroundJobs.CreateChildPermission(AppPermissions.Pages_NsPayBackgroundJobs_Delete, L("Delete"));

            var nsPaySystemSettings = nsPayManage.CreateChildPermission(AppPermissions.Pages_NsPaySystemSettings, L("NsPaySystemSettings"));
            nsPaySystemSettings.CreateChildPermission(AppPermissions.Pages_NsPaySystemSettings_Create, L("Create"));
            nsPaySystemSettings.CreateChildPermission(AppPermissions.Pages_NsPaySystemSettings_Edit, L("Edit"));
            nsPaySystemSettings.CreateChildPermission(AppPermissions.Pages_NsPaySystemSettings_Delete, L("Delete"));

            var nsPayMaintenance = nsPayManage.CreateChildPermission(AppPermissions.Pages_NsPayMaintenance, L("NsPayMaintenance"));

            #endregion

            #region 支付方式管理

            var paymentManage = napaypages.CreateChildPermission(AppPermissions.Pages_PaymentManagers, L("PaymentManager"));

            var payMents = paymentManage.CreateChildPermission(AppPermissions.Pages_PayMents, L("PayMents"));
            payMents.CreateChildPermission(AppPermissions.Pages_PayMents_Create, L("Create"));
            payMents.CreateChildPermission(AppPermissions.Pages_PayMents_Edit, L("Edit"));
            payMents.CreateChildPermission(AppPermissions.Pages_PayMents_Delete, L("Delete"));
            payMents.CreateChildPermission(AppPermissions.Pages_PayMents_ChildEdit, L("ChildEdit"));
            payMents.CreateChildPermission(AppPermissions.Pages_PayMents_OpenJob, L("OpenJobPayMent"));
            payMents.CreateChildPermission(AppPermissions.Pages_PayMents_EditStatus, L("EditStatusPayMent"));
            payMents.CreateChildPermission(AppPermissions.Pages_PayMents_GetHistory, L("GetHistoryPayMent"));

            var payGroups = paymentManage.CreateChildPermission(AppPermissions.Pages_PayGroups, L("PayGroups"));
            payGroups.CreateChildPermission(AppPermissions.Pages_PayGroups_Create, L("Create"));
            payGroups.CreateChildPermission(AppPermissions.Pages_PayGroups_Edit, L("Edit"));
            payGroups.CreateChildPermission(AppPermissions.Pages_PayGroups_Delete, L("Delete"));

            #endregion

            #region 商户管理

            var merchantManage = napaypages.CreateChildPermission(AppPermissions.Pages_MerchantManges, L("MerchantManager"));

            var merchantConfig = merchantManage.CreateChildPermission(AppPermissions.Pages_MerchantConfig, L("MerchantConfig"));
            merchantConfig.CreateChildPermission(AppPermissions.Pages_MerchantConfig_Create, L("Create"));
            merchantConfig.CreateChildPermission(AppPermissions.Pages_MerchantConfig_Edit, L("Edit"));
            merchantConfig.CreateChildPermission(AppPermissions.Pages_MerchantConfig_Delete, L("Delete"));

            var merchantSettings = merchantManage.CreateChildPermission(AppPermissions.Pages_MerchantSettings, L("MerchantSettings"));
            merchantSettings.CreateChildPermission(AppPermissions.Pages_MerchantSettings_Create, L("Create"));
            merchantSettings.CreateChildPermission(AppPermissions.Pages_MerchantSettings_Edit, L("Edit"));
            merchantSettings.CreateChildPermission(AppPermissions.Pages_MerchantSettings_Delete, L("Delete"));

            var merchantBills = merchantManage.CreateChildPermission(AppPermissions.Pages_MerchantBills, L("MerchantBills"));
            merchantBills.CreateChildPermission(AppPermissions.Pages_MerchantBills_Create, L("CreateNewMerchantBill"));
            //merchantBills.CreateChildPermission(AppPermissions.Pages_MerchantBills_Edit, L("EditMerchantBill"));
            //merchantBills.CreateChildPermission(AppPermissions.Pages_MerchantBills_Delete, L("DeleteMerchantBill"));

            var merchants = merchantManage.CreateChildPermission(AppPermissions.Pages_Merchants, L("Merchants"));
            merchants.CreateChildPermission(AppPermissions.Pages_Merchants_Create, L("Create"));
            merchants.CreateChildPermission(AppPermissions.Pages_Merchants_Edit, L("Edit"));
            merchants.CreateChildPermission(AppPermissions.Pages_Merchants_Delete, L("Delete"));
            merchants.CreateChildPermission(AppPermissions.Pages_Merchants_Recal_LockBalance, L("Recalculate_LockBalance"));


            #endregion

            #region 统计报表

            var statisticalReportManage = napaypages.CreateChildPermission(AppPermissions.Pages_StatisticalReportManage, L("StatisticalReportManage"));

            var statisticalReports = statisticalReportManage.CreateChildPermission(AppPermissions.Pages_StatisticalReports, L("StatisticalReports"));
            statisticalReports.CreateChildPermission(AppPermissions.Pages_StatisticalReports_View, L("View"));

            var statisticalBankReports = statisticalReportManage.CreateChildPermission(AppPermissions.Pages_StatisticalBankReports, L("StatisticalBankReports"));
            statisticalBankReports.CreateChildPermission(AppPermissions.Pages_StatisticalBankReports_View, L("View"));

            #endregion 

            var pages = context.GetPermissionOrNull(AppPermissions.Pages) ?? context.CreatePermission(AppPermissions.Pages, L("Pages"));

            //pages.CreateChildPermission(AppPermissions.Pages_DemoUiComponents, L("DemoUiComponents"));

            var administration = pages.CreateChildPermission(AppPermissions.Pages_Administration, L("Administration"));

            var roles = administration.CreateChildPermission(AppPermissions.Pages_Administration_Roles, L("Roles"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Create, L("CreatingNewRole"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Edit, L("EditingRole"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Delete, L("DeletingRole"));

            var users = administration.CreateChildPermission(AppPermissions.Pages_Administration_Users, L("Users"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Create, L("CreatingNewUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Edit, L("EditingUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Delete, L("DeletingUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_ChangePermissions, L("ChangingPermissions"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Impersonation, L("LoginForUsers"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Unlock, L("Unlock"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_ChangeProfilePicture, L("UpdateUsersProfilePicture"));

            var merchantUsers = administration.CreateChildPermission(AppPermissions.Pages_Merchant_Users, L("MerchantUsers"));
            merchantUsers.CreateChildPermission(AppPermissions.Pages_Merchant_Users_Create, L("CreatingNewMerchantUser"));
            merchantUsers.CreateChildPermission(AppPermissions.Pages_Merchant_Users_Edit, L("EditingMerchantUser"));
            merchantUsers.CreateChildPermission(AppPermissions.Pages_Merchant_Users_Delete, L("DeletingMerchantUser"));
            merchantUsers.CreateChildPermission(AppPermissions.Pages_Merchant_Users_Roles, L("MerchantUserRoles"));
            merchantUsers.CreateChildPermission(AppPermissions.Pages_Merchant_Users_Unlock, L("UnlockMerchantUser"));

            //RECIPIENT BANKS


            var recipientBanksManage = napaypages.CreateChildPermission(AppPermissions.Pages_RecipientBankAccountsManges, L("RecipientBanksManage"));
            var recipientBanks = recipientBanksManage.CreateChildPermission(AppPermissions.Pages_RecipientBankAccounts, L("RecipientBankAccounts"));
            recipientBanks.CreateChildPermission(AppPermissions.Pages_RecipientBankAccounts_Create, L("RecipientBankAccountsCreate"));
            recipientBanks.CreateChildPermission(AppPermissions.Pages_RecipientBankAccounts_Edit, L("RecipientBankAccountsEdit"));
            recipientBanks.CreateChildPermission(AppPermissions.Pages_RecipientBankAccounts_Delete, L("RecipientBankAccountsDelete"));


            var languages = administration.CreateChildPermission(AppPermissions.Pages_Administration_Languages, L("Languages"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Create, L("CreatingNewLanguage"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Edit, L("EditingLanguage"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Delete, L("DeletingLanguages"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_ChangeTexts, L("ChangingTexts"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_ChangeDefaultLanguage, L("ChangeDefaultLanguage"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_AuditLogs, L("AuditLogs"));

            var organizationUnits = administration.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits, L("OrganizationUnits"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree, L("ManagingOrganizationTree"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageMembers, L("ManagingMembers"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageRoles, L("ManagingRoles"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_UiCustomization, L("VisualSettings"));

            var webhooks = administration.CreateChildPermission(AppPermissions.Pages_Administration_WebhookSubscription, L("Webhooks"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_WebhookSubscription_Create, L("CreatingWebhooks"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_WebhookSubscription_Edit, L("EditingWebhooks"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_WebhookSubscription_ChangeActivity, L("ChangingWebhookActivity"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_WebhookSubscription_Detail, L("DetailingSubscription"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_Webhook_ListSendAttempts, L("ListingSendAttempts"));
            webhooks.CreateChildPermission(AppPermissions.Pages_Administration_Webhook_ResendWebhook, L("ResendingWebhook"));

            var dynamicProperties = administration.CreateChildPermission(AppPermissions.Pages_Administration_DynamicProperties, L("DynamicProperties"));
            dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicProperties_Create, L("CreatingDynamicProperties"));
            dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicProperties_Edit, L("EditingDynamicProperties"));
            dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicProperties_Delete, L("DeletingDynamicProperties"));

            var dynamicPropertyValues = dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicPropertyValue, L("DynamicPropertyValue"));
            dynamicPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicPropertyValue_Create, L("CreatingDynamicPropertyValue"));
            dynamicPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicPropertyValue_Edit, L("EditingDynamicPropertyValue"));
            dynamicPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicPropertyValue_Delete, L("DeletingDynamicPropertyValue"));

            var dynamicEntityProperties = dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityProperties, L("DynamicEntityProperties"));
            dynamicEntityProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityProperties_Create, L("CreatingDynamicEntityProperties"));
            dynamicEntityProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityProperties_Edit, L("EditingDynamicEntityProperties"));
            dynamicEntityProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityProperties_Delete, L("DeletingDynamicEntityProperties"));

            var dynamicEntityPropertyValues = dynamicProperties.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityPropertyValue, L("EntityDynamicPropertyValue"));
            dynamicEntityPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityPropertyValue_Create, L("CreatingDynamicEntityPropertyValue"));
            dynamicEntityPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityPropertyValue_Edit, L("EditingDynamicEntityPropertyValue"));
            dynamicEntityPropertyValues.CreateChildPermission(AppPermissions.Pages_Administration_DynamicEntityPropertyValue_Delete, L("DeletingDynamicEntityPropertyValue"));

            var massNotification = administration.CreateChildPermission(AppPermissions.Pages_Administration_MassNotification, L("MassNotifications"));
            massNotification.CreateChildPermission(AppPermissions.Pages_Administration_MassNotification_Create, L("MassNotificationCreate"));

            //TENANT-SPECIFIC PERMISSIONS

            pages.CreateChildPermission(AppPermissions.Pages_Tenant_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Tenant);

            administration.CreateChildPermission(AppPermissions.Pages_Administration_Tenant_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement, L("Subscription"), multiTenancySides: MultiTenancySides.Tenant);

            //HOST-SPECIFIC PERMISSIONS

            var editions = pages.CreateChildPermission(AppPermissions.Pages_Editions, L("Editions"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Create, L("CreatingNewEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Edit, L("EditingEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Delete, L("DeletingEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_MoveTenantsToAnotherEdition, L("MoveTenantsToAnotherEdition"), multiTenancySides: MultiTenancySides.Host);

            var tenants = pages.CreateChildPermission(AppPermissions.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Create, L("CreatingNewTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Edit, L("EditingTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_ChangeFeatures, L("ChangingFeatures"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Delete, L("DeletingTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Impersonation, L("LoginForTenants"), multiTenancySides: MultiTenancySides.Host);

            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Host);

            var maintenance = administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Maintenance, L("Maintenance"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            maintenance.CreateChildPermission(AppPermissions.Pages_Administration_NewVersion_Create, L("SendNewVersionNotification"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_HangfireDashboard, L("HangfireDashboard"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Host);
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, NsPayConsts.LocalizationSourceName);
        }
    }
}
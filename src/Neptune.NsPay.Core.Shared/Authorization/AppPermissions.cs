namespace Neptune.NsPay.Authorization
{
    /// <summary>
    /// Defines string constants for application's permission names.
    /// <see cref="AppAuthorizationProvider"/> for permission definitions.
    /// </summary>
    public static class AppPermissions
    {
        public const string Pages_StatisticalReportManage = "Pages.StatisticalReportManage";

        public const string Pages_StatisticalReports = "Pages.StatisticalReports";
        public const string Pages_StatisticalReports_View = "Pages.StatisticalReports.View";

        public const string Pages_StatisticalBankReports = "Pages.StatisticalBankReports";
        public const string Pages_StatisticalBankReports_View = "Pages.StatisticalBankReports.View";

        public const string Pages_RechargeOrders = "Pages.RechargeOrders";
        public const string Pages_RechargeOrders_Create = "Pages.RechargeOrders.Create";

        public const string Pages_BankBalances = "Pages.BankBalances";
        public const string Pages_BankBalances_View = "Pages.BankBalances.View";

        public const string Pages_WithdrawalOrders = "Pages.WithdrawalOrders";
        //public const string Pages_WithdrawalOrders_Create = "Pages.WithdrawalOrders.Create";
        public const string Pages_WithdrawalOrders_Edit = "Pages.WithdrawalOrders.Edit";
        //public const string Pages_WithdrawalOrders_Delete = "Pages.WithdrawalOrders.Delete";
        public const string Pages_WithdrawalOrders_CallBack = "Pages.WithdrawalOrders.CallBack";
        public const string Pages_WithdrawalOrders_View = "Pages.WithdrawalOrders.View";
        public const string Pages_WithdrawalOrders_Cancel = "Pages.WithdrawalOrders.Cancel";
        public const string Pages_WithdrawalOrders_ChangeDevice = "Pages.WithdrawalOrders.ChangeDevice";
        public const string Pages_WithdrawalOrders_ViewPayoutDetails = "Pages.WithdrawalOrders.ViewPayoutDetails";
        public const string Pages_WithdrawalOrders_ChangeToPendingStatus = "Pages.WithdrawalOrders.ChangeToPendingStatus";
        public const string Pages_WithdrawalOrders_ReleaseBalance = "Pages.WithdrawalOrders.ReleaseBalance";


        public const string Pages_WithdrawalDevices = "Pages.WithdrawalDevices";
        public const string Pages_WithdrawalDevices_Create = "Pages.WithdrawalDevices.Create";
        public const string Pages_WithdrawalDevices_Edit = "Pages.WithdrawalDevices.Edit";
        public const string Pages_WithdrawalDevices_Delete = "Pages.WithdrawalDevices.Delete";
        public const string Pages_WithdrawalDevices_ChildEdit = "Pages.WithdrawalDevices.ChildEdit";
        public const string Pages_WithdrawalDevices_BatchPauseBank = "Pages.WithdrawalDevices.BatchPauseBank";

        public const string Pages_WithdrawManagers = "Pages.WithdrawManagers";

        public const string Pages_MerchantDashboard = "Pages.MerchantDashboard";

        public const string Pages_MerchantWithdraws = "Pages.MerchantWithdraws";
        public const string Pages_MerchantWithdraws_Create = "Pages.MerchantWithdraws.Create";
        public const string Pages_MerchantWithdraws_Edit = "Pages.MerchantWithdraws.Edit";
        public const string Pages_MerchantWithdraws_Delete = "Pages.MerchantWithdraws.Delete";
        public const string Pages_MerchantWithdraws_AuditPass = "Pages.MerchantWithdraws.AuditPass";
        public const string Pages_MerchantWithdraws_AuditTurndown = "Pages.MerchantWithdraws.AuditTurndown";

        public const string Pages_MerchantWithdrawBanks = "Pages.MerchantWithdrawBanks";
        public const string Pages_MerchantWithdrawBanks_Create = "Pages.MerchantWithdrawBanks.Create";
        public const string Pages_MerchantWithdrawBanks_Edit = "Pages.MerchantWithdrawBanks.Edit";
        public const string Pages_MerchantWithdrawBanks_Delete = "Pages.MerchantWithdrawBanks.Delete";

        public const string Pages_PayOrderManagers = "Pages.PayOrderManagers";

        public const string Pages_PayOrders = "Pages.PayOrders";
        //public const string Pages_PayOrders_Create = "Pages.PayOrders.Create";
        public const string Pages_PayOrders_Edit = "Pages.PayOrders.Edit";
        //public const string Pages_PayOrders_Delete = "Pages.PayOrders.Delete";
        public const string Pages_PayOrders_CallBcak = "Pages.PayOrders.CallBcak";
        public const string Pages_PayOrders_EnforceCallBcak = "Pages.PayOrders.EnforceCallBcak";
        public const string Pages_PayOrders_ImmediateCallBcak = "Pages.PayOrders.ImmediateCallBcak";
        public const string Pages_PayOrders_View = "Pages.PayOrders.View";
        public const string Pages_PayOrders_View_BankInfo = "Pages.PayOrders.View.BankInfo";
        public const string Pages_PayOrders_AddMerchantBill = "Pages.PayOrders.AddMerchantBill";

        public const string Pages_PayOrderDeposits = "Pages.PayOrderDeposits";
        //public const string Pages_PayOrderDeposits_Create = "Pages.PayOrderDeposits.Create";
        public const string Pages_PayOrderDeposits_Edit = "Pages.PayOrderDeposits.Edit";
        //public const string Pages_PayOrderDeposits_Delete = "Pages.PayOrderDeposits.Delete";

        public const string Pages_NsPayManagers = "Pages.NsPayManagers";

        public const string Pages_NsPayBackgroundJobs = "Pages.NsPayBackgroundJobs";
        public const string Pages_NsPayBackgroundJobs_Create = "Pages.NsPayBackgroundJobs.Create";
        public const string Pages_NsPayBackgroundJobs_Edit = "Pages.NsPayBackgroundJobs.Edit";
        public const string Pages_NsPayBackgroundJobs_Delete = "Pages.NsPayBackgroundJobs.Delete";

        public const string Pages_NsPaySystemSettings = "Pages.NsPaySystemSettings";
        public const string Pages_NsPaySystemSettings_Create = "Pages.NsPaySystemSettings.Create";
        public const string Pages_NsPaySystemSettings_Edit = "Pages.NsPaySystemSettings.Edit";
        public const string Pages_NsPaySystemSettings_Delete = "Pages.NsPaySystemSettings.Delete";

        public const string Pages_NsPayMaintenance = "Pages.NsPayMaintenance";

        public const string Pages_PaymentManagers = "Pages.PaymentManagers";

        public const string Pages_PayMents = "Pages.PayMents";
        public const string Pages_PayMents_Create = "Pages.PayMents.Create";
        public const string Pages_PayMents_Edit = "Pages.PayMents.Edit";
        public const string Pages_PayMents_Delete = "Pages.PayMents.Delete";
        public const string Pages_PayMents_ChildEdit = "Pages.PayMents.ChildEdit";
        public const string Pages_PayMents_OpenJob = "Pages.PayMents.OpenJob";
        public const string Pages_PayMents_EditStatus = "Pages.PayMents.EditStatus";
        public const string Pages_PayMents_GetHistory = "Pages.PayMents.GetHistory";

        public const string Pages_PayGroups = "Pages.PayGroups";
        public const string Pages_PayGroups_Create = "Pages.PayGroups.Create";
        public const string Pages_PayGroups_Edit = "Pages.PayGroups.Edit";
        public const string Pages_PayGroups_Delete = "Pages.PayGroups.Delete";

        public const string Pages_MerchantManges = "Pages.MerchantManges";

        public const string Pages_MerchantConfig = "Pages.MerchantConfig";
        public const string Pages_MerchantConfig_Create = "Pages.MerchantConfig.Create";
        public const string Pages_MerchantConfig_Edit = "Pages.MerchantConfig.Edit";
        public const string Pages_MerchantConfig_Delete = "Pages.MerchantConfig.Delete";

        public const string Pages_MerchantSettings = "Pages.MerchantSettings";
        public const string Pages_MerchantSettings_Create = "Pages.MerchantSettings.Create";
        public const string Pages_MerchantSettings_Edit = "Pages.MerchantSettings.Edit";
        public const string Pages_MerchantSettings_Delete = "Pages.MerchantSettings.Delete";

        public const string Pages_MerchantBills = "Pages.MerchantBills";
        public const string Pages_MerchantBills_Create = "Pages.MerchantBills.Create";
        //public const string Pages_MerchantBills_Edit = "Pages.MerchantBills.Edit";
        //public const string Pages_MerchantBills_Delete = "Pages.MerchantBills.Delete";

        public const string Pages_MerchantFunds = "Pages.MerchantFunds";
        public const string Pages_MerchantFunds_Create = "Pages.MerchantFunds.Create";
        public const string Pages_MerchantFunds_Edit = "Pages.MerchantFunds.Edit";
        public const string Pages_MerchantFunds_Delete = "Pages.MerchantFunds.Delete";

        public const string Pages_MerchantRates = "Pages.MerchantRates";
        public const string Pages_MerchantRates_Create = "Pages.MerchantRates.Create";
        public const string Pages_MerchantRates_Edit = "Pages.MerchantRates.Edit";
        public const string Pages_MerchantRates_Delete = "Pages.MerchantRates.Delete";

        public const string Pages_Merchants = "Pages.Merchants";
        public const string Pages_Merchants_Create = "Pages.Merchants.Create";
        public const string Pages_Merchants_Edit = "Pages.Merchants.Edit";
        public const string Pages_Merchants_Delete = "Pages.Merchants.Delete";
        public const string Pages_Merchants_Recal_LockBalance = "Pages.Merchants.Recal_LockBalance";


        //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)

        public const string NsPayPages = "NsPayPages";

        public const string Pages = "Pages";

        public const string Pages_DemoUiComponents = "Pages.DemoUiComponents";
        public const string Pages_Administration = "Pages.Administration";

        public const string Pages_Administration_Roles = "Pages.Administration.Roles";
        public const string Pages_Administration_Roles_Create = "Pages.Administration.Roles.Create";
        public const string Pages_Administration_Roles_Edit = "Pages.Administration.Roles.Edit";
        public const string Pages_Administration_Roles_Delete = "Pages.Administration.Roles.Delete";

        public const string Pages_Administration_Users = "Pages.Administration.Users";
        public const string Pages_Administration_Users_Create = "Pages.Administration.Users.Create";
        public const string Pages_Administration_Users_Edit = "Pages.Administration.Users.Edit";
        public const string Pages_Administration_Users_Delete = "Pages.Administration.Users.Delete";
        public const string Pages_Administration_Users_ChangePermissions = "Pages.Administration.Users.ChangePermissions";
        public const string Pages_Administration_Users_Impersonation = "Pages.Administration.Users.Impersonation";
        public const string Pages_Administration_Users_Unlock = "Pages.Administration.Users.Unlock";
        public const string Pages_Administration_Users_ChangeProfilePicture = "Pages.Administration.Users.ChangeProfilePicture";

        public const string Pages_Administration_Languages = "Pages.Administration.Languages";
        public const string Pages_Administration_Languages_Create = "Pages.Administration.Languages.Create";
        public const string Pages_Administration_Languages_Edit = "Pages.Administration.Languages.Edit";
        public const string Pages_Administration_Languages_Delete = "Pages.Administration.Languages.Delete";
        public const string Pages_Administration_Languages_ChangeTexts = "Pages.Administration.Languages.ChangeTexts";
        public const string Pages_Administration_Languages_ChangeDefaultLanguage = "Pages.Administration.Languages.ChangeDefaultLanguage";

        public const string Pages_Administration_AuditLogs = "Pages.Administration.AuditLogs";

        public const string Pages_Administration_OrganizationUnits = "Pages.Administration.OrganizationUnits";
        public const string Pages_Administration_OrganizationUnits_ManageOrganizationTree = "Pages.Administration.OrganizationUnits.ManageOrganizationTree";
        public const string Pages_Administration_OrganizationUnits_ManageMembers = "Pages.Administration.OrganizationUnits.ManageMembers";
        public const string Pages_Administration_OrganizationUnits_ManageRoles = "Pages.Administration.OrganizationUnits.ManageRoles";

        public const string Pages_Administration_HangfireDashboard = "Pages.Administration.HangfireDashboard";

        public const string Pages_Administration_UiCustomization = "Pages.Administration.UiCustomization";

        public const string Pages_Administration_WebhookSubscription = "Pages.Administration.WebhookSubscription";
        public const string Pages_Administration_WebhookSubscription_Create = "Pages.Administration.WebhookSubscription.Create";
        public const string Pages_Administration_WebhookSubscription_Edit = "Pages.Administration.WebhookSubscription.Edit";
        public const string Pages_Administration_WebhookSubscription_ChangeActivity = "Pages.Administration.WebhookSubscription.ChangeActivity";
        public const string Pages_Administration_WebhookSubscription_Detail = "Pages.Administration.WebhookSubscription.Detail";
        public const string Pages_Administration_Webhook_ListSendAttempts = "Pages.Administration.Webhook.ListSendAttempts";
        public const string Pages_Administration_Webhook_ResendWebhook = "Pages.Administration.Webhook.ResendWebhook";

        public const string Pages_Administration_DynamicProperties = "Pages.Administration.DynamicProperties";
        public const string Pages_Administration_DynamicProperties_Create = "Pages.Administration.DynamicProperties.Create";
        public const string Pages_Administration_DynamicProperties_Edit = "Pages.Administration.DynamicProperties.Edit";
        public const string Pages_Administration_DynamicProperties_Delete = "Pages.Administration.DynamicProperties.Delete";

        public const string Pages_Administration_DynamicPropertyValue = "Pages.Administration.DynamicPropertyValue";
        public const string Pages_Administration_DynamicPropertyValue_Create = "Pages.Administration.DynamicPropertyValue.Create";
        public const string Pages_Administration_DynamicPropertyValue_Edit = "Pages.Administration.DynamicPropertyValue.Edit";
        public const string Pages_Administration_DynamicPropertyValue_Delete = "Pages.Administration.DynamicPropertyValue.Delete";

        public const string Pages_Administration_DynamicEntityProperties = "Pages.Administration.DynamicEntityProperties";
        public const string Pages_Administration_DynamicEntityProperties_Create = "Pages.Administration.DynamicEntityProperties.Create";
        public const string Pages_Administration_DynamicEntityProperties_Edit = "Pages.Administration.DynamicEntityProperties.Edit";
        public const string Pages_Administration_DynamicEntityProperties_Delete = "Pages.Administration.DynamicEntityProperties.Delete";

        public const string Pages_Administration_DynamicEntityPropertyValue = "Pages.Administration.DynamicEntityPropertyValue";
        public const string Pages_Administration_DynamicEntityPropertyValue_Create = "Pages.Administration.DynamicEntityPropertyValue.Create";
        public const string Pages_Administration_DynamicEntityPropertyValue_Edit = "Pages.Administration.DynamicEntityPropertyValue.Edit";
        public const string Pages_Administration_DynamicEntityPropertyValue_Delete = "Pages.Administration.DynamicEntityPropertyValue.Delete";

        public const string Pages_Administration_MassNotification = "Pages.Administration.MassNotification";
        public const string Pages_Administration_MassNotification_Create = "Pages.Administration.MassNotification.Create";

        public const string Pages_Administration_NewVersion_Create = "Pages_Administration_NewVersion_Create";

        //TENANT-SPECIFIC PERMISSIONS

        public const string Pages_Tenant_Dashboard = "Pages.Tenant.Dashboard";

        public const string Pages_Administration_Tenant_Settings = "Pages.Administration.Tenant.Settings";

        public const string Pages_Administration_Tenant_SubscriptionManagement = "Pages.Administration.Tenant.SubscriptionManagement";

        //HOST-SPECIFIC PERMISSIONS

        public const string Pages_Editions = "Pages.Editions";
        public const string Pages_Editions_Create = "Pages.Editions.Create";
        public const string Pages_Editions_Edit = "Pages.Editions.Edit";
        public const string Pages_Editions_Delete = "Pages.Editions.Delete";
        public const string Pages_Editions_MoveTenantsToAnotherEdition = "Pages.Editions.MoveTenantsToAnotherEdition";

        public const string Pages_Tenants = "Pages.Tenants";
        public const string Pages_Tenants_Create = "Pages.Tenants.Create";
        public const string Pages_Tenants_Edit = "Pages.Tenants.Edit";
        public const string Pages_Tenants_ChangeFeatures = "Pages.Tenants.ChangeFeatures";
        public const string Pages_Tenants_Delete = "Pages.Tenants.Delete";
        public const string Pages_Tenants_Impersonation = "Pages.Tenants.Impersonation";

        public const string Pages_Administration_Host_Maintenance = "Pages.Administration.Host.Maintenance";
        public const string Pages_Administration_Host_Settings = "Pages.Administration.Host.Settings";
        public const string Pages_Administration_Host_Dashboard = "Pages.Administration.Host.Dashboard";


        //merchant Account 
        public const string Pages_Merchant_Users = "Pages.Merchant.Users";
        public const string Pages_Merchant_Users_Create = "Pages.Merchant.Users.Create";
        public const string Pages_Merchant_Users_Edit = "Pages.Merchant.Users.Edit";
        public const string Pages_Merchant_Users_Delete = "Pages.Merchant.Users.Delete";
        public const string Pages_Merchant_Users_Roles = "Pages.Merchant.Users.Roles";
        public const string Pages_Merchant_Users_Unlock = "Pages.Merchant.Users.Unlock";


        //recipientbankaccounts
        public const string Pages_RecipientBankAccountsManges = "Pages.RecipientBankAccountsManages";
        public const string Pages_RecipientBankAccounts = "Pages.RecipientBankAccounts";
        public const string Pages_RecipientBankAccounts_Edit = "Pages.RecipientBankAccounts.Edit";
        public const string Pages_RecipientBankAccounts_Create = "Pages.RecipientBankAccounts.Create";
        public const string Pages_RecipientBankAccounts_Delete = "Pages.RecipientBankAccounts.Delete";

    }
}
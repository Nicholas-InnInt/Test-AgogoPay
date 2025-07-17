using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using Neptune.NsPay.Authorization;

namespace Neptune.NsPay.Web.Areas.AppArea.Startup
{
    public class AppAreaNavigationProvider : NavigationProvider
    {
        public const string MenuName = "App";

        public override void SetNavigation(INavigationProviderContext context)
        {
            var menu = context.Manager.Menus[MenuName] = new MenuDefinition(MenuName, new FixedLocalizableString("Main Menu"));

            menu
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Host.Dashboard,
                        L("Dashboard"),
                        url: "AppArea/HostDashboard",
                        icon: "flaticon-line-graph",
                        permissionDependency: SP(AppPermissions.Pages_Administration_Host_Dashboard)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Host.Tenants,
                        L("Tenants"),
                        url: "AppArea/Tenants",
                        icon: "flaticon-list-3",
                        permissionDependency: SP(AppPermissions.Pages_Tenants)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Host.Editions,
                        L("Editions"),
                        url: "AppArea/Editions",
                        icon: "flaticon-app",
                        permissionDependency: SP(AppPermissions.Pages_Editions)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Tenant.Dashboard,
                        L("Dashboard"),
                        url: "AppArea/TenantDashboard",
                        icon: "flaticon-line-graph",
                        permissionDependency: SP(AppPermissions.Pages_Tenant_Dashboard)
                    )
                )
                .AddItem(AdministrationMenuItem())
                .AddItem(MerchantMenuItem())
                .AddItem(PaymentMenuItem())
                .AddItem(PayOrderMenuItem())
                .AddItem(PayOrderCryptoMenuItem())
                .AddItem(WithdrawalMenuItem())
                .AddItem(RecipientBankMenuItem())
                .AddItem(StatisticalReportMenuItem())
                .AddItem(SystemMenuItem());
        }

        private static MenuItemDefinition AdministrationMenuItem()
        {
            return new MenuItemDefinition(AppAreaPageNames.Common.Administration, L("Administration"), icon: "flaticon-interface-8")
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.OrganizationUnits,
                        L("OrganizationUnits"),
                        url: "AppArea/OrganizationUnits",
                        icon: "flaticon-map",
                        permissionDependency: SP(AppPermissions.Pages_Administration_OrganizationUnits)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.Roles,
                        L("Roles"),
                        url: "AppArea/Roles",
                        icon: "flaticon-suitcase",
                        permissionDependency: SP(AppPermissions.Pages_Administration_Roles)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.Users,
                        L("Users"),
                        url: "AppArea/Users",
                        icon: "flaticon-users",
                        permissionDependency: SP(AppPermissions.Pages_Administration_Users)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.Languages,
                        L("Languages"),
                        url: "AppArea/Languages",
                        icon: "flaticon-tabs",
                        permissionDependency: SP(AppPermissions.Pages_Administration_Languages)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.AuditLogs,
                        L("AuditLogs"),
                        url: "AppArea/AuditLogs",
                        icon: "flaticon-folder-1",
                        permissionDependency: SP(AppPermissions.Pages_Administration_AuditLogs)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Host.Maintenance,
                        L("Maintenance"),
                        url: "AppArea/Maintenance",
                        icon: "flaticon-lock",
                        permissionDependency: SP(AppPermissions.Pages_Administration_Host_Maintenance)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Tenant.SubscriptionManagement,
                        L("Subscription"),
                        url: "AppArea/SubscriptionManagement",
                        icon: "flaticon-refresh",
                        permissionDependency: SP(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.UiCustomization,
                        L("VisualSettings"),
                        url: "AppArea/UiCustomization",
                        icon: "flaticon-medical",
                        permissionDependency: SP(AppPermissions.Pages_Administration_UiCustomization)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.WebhookSubscriptions,
                        L("WebhookSubscriptions"),
                        url: "AppArea/WebhookSubscription",
                        icon: "flaticon2-world",
                        permissionDependency: SP(AppPermissions.Pages_Administration_WebhookSubscription)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.DynamicProperties,
                        L("DynamicProperties"),
                        url: "AppArea/DynamicProperty",
                        icon: "flaticon-interface-8",
                        permissionDependency: SP(AppPermissions.Pages_Administration_DynamicProperties)
                    )
                )
                .AddItem(new MenuItemDefinition(AppAreaPageNames.Common.Notifications, L("Notifications"), icon: "flaticon-alarm")
                    .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.Notifications_Inbox,
                        L("Inbox"),
                        url: "AppArea/Notifications",
                        icon: "flaticon-mail-1")
                    )
                    .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.Notifications_MassNotifications,
                        L("MassNotifications"),
                        url: "AppArea/Notifications/MassNotifications",
                        icon: "flaticon-paper-plane",
                        permissionDependency: SP(AppPermissions.Pages_Administration_MassNotification))
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Host.Settings,
                        L("Settings"),
                        url: "AppArea/HostSettings",
                        icon: "flaticon-settings",
                        permissionDependency: SP(AppPermissions.Pages_Administration_Host_Settings)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Tenant.Settings,
                        L("Settings"),
                        url: "AppArea/Settings",
                        icon: "flaticon-settings",
                        permissionDependency: SP(AppPermissions.Pages_Administration_Tenant_Settings)
                    )
                );
        }

        private static MenuItemDefinition MerchantMenuItem()
        {
            return new MenuItemDefinition(AppAreaPageNames.Merchant.MerchantManager, L("MerchantManager"), icon: "flaticon-shapes")
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Merchant.MerchantDashboard,
                        L("MerchantDashboard"),
                        url: "AppArea/MerchantDashboard",
                        icon: "flaticon-more",
                        permissionDependency:
                        SP(AppPermissions.Pages_MerchantDashboard)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Merchant.Merchants,
                        L("Merchants"),
                        url: "AppArea/Merchants",
                        icon: "flaticon-more",
                        permissionDependency:
                        SP(AppPermissions.Pages_Merchants)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Merchant.MerchantSettings,
                        L("MerchantSettings"),
                        url: "AppArea/MerchantSettings",
                        icon: "flaticon-more",
                        permissionDependency:
                        SP(AppPermissions.Pages_MerchantSettings)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Common.MerchantUsers,
                        L("MerchantUsers"),
                        url: "AppArea/Users/Merchant",
                        icon: "flaticon-users",
                        permissionDependency:
                        SP(AppPermissions.Pages_Merchant_Users)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Merchant.MerchantConfig,
                        L("MerchantConfig"),
                        url: "AppArea/MerchantConfig",
                        icon: "flaticon-more",
                        permissionDependency:
                        SP(AppPermissions.Pages_MerchantConfig)
                    )
                );
        }

        private static MenuItemDefinition PaymentMenuItem()
        {
            return new MenuItemDefinition(AppAreaPageNames.PayMent.PaymentManager, L("PaymentManager"), icon: "flaticon-shapes")
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.PayMent.PayMents,
                        L("PayMents"),
                        url: "AppArea/PayMents",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_PayMents)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.PayMent.PayGroups,
                        L("PayGroups"),
                        url: "AppArea/PayGroups",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_PayGroups)
                    )
                );
        }

        private static MenuItemDefinition PayOrderMenuItem()
        {
            return new MenuItemDefinition(AppAreaPageNames.PayOrder.PayOrderManager, L("PayOrderManager"), icon: "flaticon-shapes")
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.PayOrder.PayOrders,
                        L("PayOrders"),
                        url: "AppArea/PayOrders",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_PayOrders)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.PayOrder.PayOrderDeposits,
                        L("PayOrderDeposits"),
                        url: "AppArea/PayOrderDeposits",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_PayOrderDeposits)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.PayOrder.MerchantBills,
                        L("MerchantBills"),
                        url: "AppArea/MerchantBills",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_MerchantBills)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.PayOrder.BankBalances,
                        L("BankBalances"),
                        url: "AppArea/BankBalance",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_BankBalances)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.PayOrder.RechargeOrders,
                        L("RechargeOrders"),
                        url: "AppArea/RechargeOrders",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_RechargeOrders)
                    )
                );
        }

        private static MenuItemDefinition PayOrderCryptoMenuItem()
        {
            return new MenuItemDefinition(AppAreaPageNames.PayOrder.PayOrderCryptoManager, L("PayOrderCryptoManager"), icon: "flaticon-shapes")
               .AddItem(new MenuItemDefinition(
                       AppAreaPageNames.PayOrder.PayOrdersCrypto,
                       L("PayOrders"),
                       url: "AppArea/PayOrdersCrypto",
                       icon: "flaticon-more",
                       permissionDependency: SP(AppPermissions.Pages_PayOrders)
                   )
               )
               .AddItem(new MenuItemDefinition(
                       AppAreaPageNames.PayOrder.PayOrderDepositsCrypto,
                       L("PayOrderDeposits"),
                       url: "AppArea/PayOrderDepositsCrypto",
                       icon: "flaticon-more",
                       permissionDependency: SP(AppPermissions.Pages_PayOrderDeposits)
                   )
               )
               .AddItem(new MenuItemDefinition(
                       AppAreaPageNames.PayOrder.MerchantBillsCrypto,
                       L("MerchantBills"),
                       url: "AppArea/MerchantBillsCrypto",
                       icon: "flaticon-more",
                       permissionDependency: SP(AppPermissions.Pages_MerchantBills)
                   )
               );
        }

        private static MenuItemDefinition WithdrawalMenuItem()
        {
            return new MenuItemDefinition(AppAreaPageNames.Withdraw.WithdrawManager, L("WithdrawManager"), icon: "flaticon-shapes")
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Withdraw.MerchantWithdraws,
                        L("MerchantWithdraws"),
                        url: "AppArea/MerchantWithdraws",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_MerchantWithdraws)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Withdraw.MerchantWithdrawBanks,
                        L("MerchantWithdrawBanks"),
                        url: "AppArea/MerchantWithdrawBanks",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_MerchantWithdrawBanks)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Withdraw.WithdrawalDevices,
                        L("WithdrawalDevices"),
                        url: "AppArea/WithdrawalDevices",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_WithdrawalDevices)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.Withdraw.WithdrawalOrders,
                        L("WithdrawalOrders"),
                        url: "AppArea/WithdrawalOrders",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_WithdrawalOrders)
                    )
                );
        }

        private static MenuItemDefinition RecipientBankMenuItem()
        {
            return new MenuItemDefinition(AppAreaPageNames.RecipientBankAccounts.RecipientBankAccountsDashBoard, L("RecipientBankAccountsManager"), icon: "flaticon-shapes")
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.RecipientBankAccounts.RecipientBankAccountsManage,
                        L("RecipientBankAccounts"),
                        url: "AppArea/RecipientBankAccounts",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_RecipientBankAccountsManges)
                    )
                );
        }

        private static MenuItemDefinition StatisticalReportMenuItem()
        {
            return new MenuItemDefinition(AppAreaPageNames.StatisticalReport.StatisticalReportManage, L("StatisticalReportManage"), icon: "flaticon-shapes")
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.StatisticalReport.StatisticalReports,
                        L("StatisticalReports"),
                        url: "AppArea/StatisticalReports",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_StatisticalReports)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.StatisticalReport.StatisticalBankReports,
                        L("StatisticalBankReports"),
                        url: "AppArea/StatisticalBankReports",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_StatisticalBankReports)
                    )
                );
        }

        private static MenuItemDefinition SystemMenuItem()
        {
            return new MenuItemDefinition(AppAreaPageNames.NsPays.NsPayManager, L("NsPayManager"), icon: "flaticon-shapes")
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.NsPays.NsPaySystemSettings,
                        L("NsPaySystemSettings"),
                        url: "AppArea/NsPaySystemSettings",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_NsPaySystemSettings)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.NsPays.NsPayBackgroundJobs,
                        L("NsPayBackgroundJobs"),
                        url: "AppArea/NsPayBackgroundJobs",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_NsPayBackgroundJobs)
                    )
                )
                .AddItem(new MenuItemDefinition(
                        AppAreaPageNames.NsPays.NsPayMaintenance,
                        L("NsPayMaintenance"),
                        url: "AppArea/NsPayMaintenance",
                        icon: "flaticon-more",
                        permissionDependency: SP(AppPermissions.Pages_NsPayMaintenance)
                    )
                );
        }

        #region Helper Methods

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, NsPayConsts.LocalizationSourceName);
        }

        private static SimplePermissionDependency SP(params string[] permissions)
        {
            return new SimplePermissionDependency(permissions);
        }

        #endregion Helper Methods
    }
}
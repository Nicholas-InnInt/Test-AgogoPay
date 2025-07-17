namespace Neptune.NsPay.Web.Areas.AppArea.Startup
{
    public class AppAreaPageNames
    {
        public static class Common
        {
            public const string AbpUserMerchants = "AbpUserMerchants.AbpUserMerchants";
            public const string Administration = "Administration";
            public const string Roles = "Administration.Roles";
            public const string Users = "Administration.Users";
            public const string MerchantUsers = "Merchant.Users";
            public const string AuditLogs = "Administration.AuditLogs";
            public const string OrganizationUnits = "Administration.OrganizationUnits";
            public const string Languages = "Administration.Languages";
            public const string DemoUiComponents = "Administration.DemoUiComponents";
            public const string UiCustomization = "Administration.UiCustomization";
            public const string WebhookSubscriptions = "Administration.WebhookSubscriptions";
            public const string DynamicProperties = "Administration.DynamicProperties";
            public const string DynamicEntityProperties = "Administration.DynamicEntityProperties";
            public const string Notifications = "Administration.Notifications";
            public const string Notifications_Inbox = "Administration.Notifications.Inbox";
            public const string Notifications_MassNotifications = "Administration.Notifications.MassNotifications";
        }

        public static class Host
        {
            public const string Tenants = "Tenants";
            public const string Editions = "Editions";
            public const string Maintenance = "Administration.Maintenance";
            public const string Settings = "Administration.Settings.Host";
            public const string Dashboard = "HostDashboard";
        }

        public static class Tenant
        {
            public const string Dashboard = "TenantDashboard";
            public const string Settings = "Administration.Settings.Tenant";
            public const string SubscriptionManagement = "Administration.SubscriptionManagement.Tenant";
        }

        public static class Merchant
        {
            public const string MerchantManager = "MerchantManager";
            public const string MerchantDashboard = "MerchantDashboard";
            public const string MerchantFunds = "MerchantFunds.MerchantFunds";
            public const string MerchantRates = "MerchantRates.MerchantRates";
            public const string Merchants = "Merchants.Merchants";
            public const string MerchantSettings = "MerchantSettings.MerchantSettings";
            public const string MerchantConfig = "MerchantConfig.MerchantConfig";
        }

        public static class PayMent
        {
            public const string PaymentManager = "PaymentManager";
            public const string PayMents = "PayMents.PayMents";
            public const string PayGroups = "PayGroups.PayGroups";
            public const string PayGroupMents = "PayGroupMents.PayGroupMents";
        }

        public static class NsPays
        {
            public const string NsPayManager = "NsPayManager";
            public const string NsPaySystemSettings = "NsPaySystemSettings.NsPaySystemSettings";
            public const string NsPayBackgroundJobs = "NsPayBackgroundJobs.NsPayBackgroundJobs";
            public const string NsPayMaintenance = "NsPayMaintenance.NsPayMaintenance";
        }

        public static class PayOrder
        {
            public const string PayOrderManager = "PayOrderManager";
            public const string PayOrders = "PayOrders.PayOrders";
            public const string PayOrderDeposits = "PayOrderDeposits.PayOrderDeposits";
            public const string MerchantBills = "MerchantBills.MerchantBills";
            public const string BankBalances = "Merchants.BankBalances";
            public const string RechargeOrders = "PayOrders.RechargeOrders";

            // Crypto
            public const string PayOrderCryptoManager = "PayOrderCryptoManager";
            public const string PayOrdersCrypto = "PayOrders.PayOrdersCrypto";
            public const string PayOrderDepositsCrypto = "PayOrderDeposits.PayOrderDepositsCrypto";
            public const string MerchantBillsCrypto = "MerchantBills.MerchantBillsCrypto";
            public const string BankBalancesCrypto = "Merchants.BankBalancesCrypto";
            public const string RechargeOrdersCrypto = "PayOrders.RechargeOrdersCrypto";
        }

        public static class Withdraw
        {
            public const string WithdrawManager = "WithdrawManager";
            public const string MerchantWithdraws = "MerchantWithdraws.MerchantWithdraws";
            public const string MerchantWithdrawBanks = "MerchantWithdrawBanks.MerchantWithdrawBanks";
            public const string WithdrawalOrders = "WithdrawalOrders.WithdrawalOrders";
            public const string WithdrawalDevices = "WithdrawalDevices.WithdrawalDevices";
        }
        public static class StatisticalReport
        {
            public const string StatisticalReportManage = "StatisticalReportManage";
            public const string StatisticalReports = "StatisticalReports";
            public const string StatisticalBankReports = "StatisticalBankReports";
        }

        public static class RecipientBankAccounts
        {
            public const string RecipientBankAccountsDashBoard = "RecipientBankAccountsDashBoard";
            public const string RecipientBankAccountsManage = "RecipientBankAccountsManage";
        }
    }
}
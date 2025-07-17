using System.Collections.Generic;
using Neptune.NsPay.Web.DashboardCustomization;


namespace Neptune.NsPay.Web.Areas.AppArea.Startup
{
    public class DashboardViewConfiguration
    {
        public Dictionary<string, WidgetViewDefinition> WidgetViewDefinitions { get; } = new Dictionary<string, WidgetViewDefinition>();

        public Dictionary<string, WidgetFilterViewDefinition> WidgetFilterViewDefinitions { get; } = new Dictionary<string, WidgetFilterViewDefinition>();

        public DashboardViewConfiguration()
        {
            var jsAndCssFileRoot = "/Areas/AppArea/Views/CustomizableDashboard/Widgets/";
            var viewFileRoot = "AppArea/Widgets/";

            #region FilterViewDefinitions

            WidgetFilterViewDefinitions.Add(NsPayDashboardCustomizationConsts.Filters.FilterDateRangePicker,
                new WidgetFilterViewDefinition(
                    NsPayDashboardCustomizationConsts.Filters.FilterDateRangePicker,
                    "~/Areas/AppArea/Views/Shared/Components/CustomizableDashboard/Widgets/DateRangeFilter.cshtml",
                    jsAndCssFileRoot + "DateRangeFilter/DateRangeFilter.min.js",
                    jsAndCssFileRoot + "DateRangeFilter/DateRangeFilter.min.css")
            );

            //add your filters iew definitions here
            #endregion

            #region WidgetViewDefinitions

            #region TenantWidgets

            //WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Tenant.DailySales,
            //    new WidgetViewDefinition(
            //        NsPayDashboardCustomizationConsts.Widgets.Tenant.DailySales,
            //        viewFileRoot + "DailySales",
            //        jsAndCssFileRoot + "DailySales/DailySales.min.js",
            //        jsAndCssFileRoot + "DailySales/DailySales.min.css"));

            //WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Tenant.GeneralStats,
            //    new WidgetViewDefinition(
            //        NsPayDashboardCustomizationConsts.Widgets.Tenant.GeneralStats,
            //        viewFileRoot + "GeneralStats",
            //        jsAndCssFileRoot + "GeneralStats/GeneralStats.min.js",
            //        jsAndCssFileRoot + "GeneralStats/GeneralStats.min.css"));

            //WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Tenant.ProfitShare,
            //    new WidgetViewDefinition(
            //        NsPayDashboardCustomizationConsts.Widgets.Tenant.ProfitShare,
            //        viewFileRoot + "ProfitShare",
            //        jsAndCssFileRoot + "ProfitShare/ProfitShare.min.js",
            //        jsAndCssFileRoot + "ProfitShare/ProfitShare.min.css"));

            //WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Tenant.MemberActivity,
            //    new WidgetViewDefinition(
            //        NsPayDashboardCustomizationConsts.Widgets.Tenant.MemberActivity,
            //        viewFileRoot + "MemberActivity",
            //        jsAndCssFileRoot + "MemberActivity/MemberActivity.min.js",
            //        jsAndCssFileRoot + "MemberActivity/MemberActivity.min.css"));

            //WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Tenant.RegionalStats,
            //    new WidgetViewDefinition(
            //        NsPayDashboardCustomizationConsts.Widgets.Tenant.RegionalStats,
            //        viewFileRoot + "RegionalStats",
            //        jsAndCssFileRoot + "RegionalStats/RegionalStats.min.js",
            //        jsAndCssFileRoot + "RegionalStats/RegionalStats.min.css",
            //        12,
            //        10));

            //WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Tenant.SalesSummary,
            //    new WidgetViewDefinition(
            //        NsPayDashboardCustomizationConsts.Widgets.Tenant.SalesSummary,
            //        viewFileRoot + "SalesSummary",
            //        jsAndCssFileRoot + "SalesSummary/SalesSummary.min.js",
            //        jsAndCssFileRoot + "SalesSummary/SalesSummary.min.css",
            //        12,
            //        10));

            WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Tenant.MerchantBillsSummary,
                new WidgetViewDefinition(
                    NsPayDashboardCustomizationConsts.Widgets.Tenant.MerchantBillsSummary,
                    viewFileRoot + "MerchantBillsSummary",
                    jsAndCssFileRoot + "MerchantBillsSummary/MerchantBillsSummary.min.js",
                    jsAndCssFileRoot + "MerchantBillsSummary/MerchantBillsSummary.min.css",
                    12,
                    10));


            WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Tenant.TopStats,
                new WidgetViewDefinition(
                    NsPayDashboardCustomizationConsts.Widgets.Tenant.TopStats,
                    viewFileRoot + "TopStats",
                    jsAndCssFileRoot + "TopStats/TopStats.min.js",
                    jsAndCssFileRoot + "TopStats/TopStats.min.css",
                    12,
                    10));

            //add your tenant side widget definitions here
            #endregion

            #region HostWidgets

            //WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Host.IncomeStatistics,
            //    new WidgetViewDefinition(
            //        NsPayDashboardCustomizationConsts.Widgets.Host.IncomeStatistics,
            //        viewFileRoot + "IncomeStatistics",
            //        jsAndCssFileRoot + "IncomeStatistics/IncomeStatistics.min.js",
            //        jsAndCssFileRoot + "IncomeStatistics/IncomeStatistics.min.css"));

            WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Host.TopStats,
                new WidgetViewDefinition(
                    NsPayDashboardCustomizationConsts.Widgets.Host.TopStats,
                    viewFileRoot + "HostTopStats",
                    jsAndCssFileRoot + "HostTopStats/HostTopStats.min.js",
                    jsAndCssFileRoot + "HostTopStats/HostTopStats.min.css"));

            //WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Host.EditionStatistics,
            //    new WidgetViewDefinition(
            //        NsPayDashboardCustomizationConsts.Widgets.Host.EditionStatistics,
            //        viewFileRoot + "EditionStatistics",
            //        jsAndCssFileRoot + "EditionStatistics/EditionStatistics.min.js",
            //        jsAndCssFileRoot + "EditionStatistics/EditionStatistics.min.css"));

            //WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Host.SubscriptionExpiringTenants,
            //    new WidgetViewDefinition(
            //        NsPayDashboardCustomizationConsts.Widgets.Host.SubscriptionExpiringTenants,
            //        viewFileRoot + "SubscriptionExpiringTenants",
            //        jsAndCssFileRoot + "SubscriptionExpiringTenants/SubscriptionExpiringTenants.min.js",
            //        jsAndCssFileRoot + "SubscriptionExpiringTenants/SubscriptionExpiringTenants.min.css",
            //        6,
            //        10));

            //WidgetViewDefinitions.Add(NsPayDashboardCustomizationConsts.Widgets.Host.RecentTenants,
            //    new WidgetViewDefinition(
            //        NsPayDashboardCustomizationConsts.Widgets.Host.RecentTenants,
            //        viewFileRoot + "RecentTenants",
            //        jsAndCssFileRoot + "RecentTenants/RecentTenants.min.js",
            //        jsAndCssFileRoot + "RecentTenants/RecentTenants.min.css"));

            //add your host side widgets definitions here
            #endregion

            #endregion
        }
    }
}

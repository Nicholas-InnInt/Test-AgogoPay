using Abp.Application.Services;
using Neptune.NsPay.Tenants.Dashboard.Dto;
using System;
using System.Threading.Tasks;

namespace Neptune.NsPay.Tenants.Dashboard
{
    public interface ITenantDashboardAppService : IApplicationService
    {
        GetMemberActivityOutput GetMemberActivity();

        GetDashboardDataOutput GetDashboardData(GetDashboardDataInput input);

        GetDailySalesOutput GetDailySales();

        GetProfitShareOutput GetProfitShare();

        GetSalesSummaryOutput GetSalesSummary(GetSalesSummaryInput input);
        Task<GetMerchantBillsSummaryOutput> GetMerchantBillsSummary(GetMerchantBillsSummaryInput input);
        Task<GetTopStatsOutput> GetTopStats(DateTime startDate, DateTime endDate);

        GetRegionalStatsOutput GetRegionalStats();

        GetGeneralStatsOutput GetGeneralStats();
    }
}

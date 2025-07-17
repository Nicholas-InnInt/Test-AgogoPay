using System.Threading.Tasks;
using Abp.Application.Services;
using Neptune.NsPay.MultiTenancy.HostDashboard.Dto;

namespace Neptune.NsPay.MultiTenancy.HostDashboard
{
    public interface IHostDashboardAppService : IApplicationService
    {
        Task<TopStatsData> GetTopStatsData(GetTopStatsInput input);

        Task<GetRecentTenantsOutput> GetRecentTenantsData();

        Task<GetExpiringTenantsOutput> GetSubscriptionExpiringTenantsData();

        Task<GetIncomeStatisticsDataOutput> GetIncomeStatistics(GetIncomeStatisticsDataInput input);

        Task<GetEditionTenantStatisticsOutput> GetEditionTenantStatistics(GetEditionTenantStatisticsInput input);
    }
}
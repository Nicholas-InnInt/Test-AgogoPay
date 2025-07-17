namespace Neptune.NsPay.MultiTenancy.HostDashboard.Dto
{
    public class GetIncomeStatisticsDataInput : DashboardInputBase
    {
        public ChartDateInterval IncomeStatisticsDateInterval { get; set; }
    }
}
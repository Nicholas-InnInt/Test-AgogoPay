using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neptune.NsPay.MultiTenancy.HostDashboard.Dto;

namespace Neptune.NsPay.MultiTenancy.HostDashboard
{
    public interface IIncomeStatisticsService
    {
        Task<List<IncomeStastistic>> GetIncomeStatisticsData(DateTime startDate, DateTime endDate,
            ChartDateInterval dateInterval);
    }
}
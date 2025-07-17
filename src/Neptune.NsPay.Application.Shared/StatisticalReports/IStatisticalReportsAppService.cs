using Abp.Application.Services;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.StatisticalReports
{
    public interface IStatisticalReportsAppService: IApplicationService
    {
        Task<IList<GetOrderMerchantViewDto>> GetMerchants();

    }
}

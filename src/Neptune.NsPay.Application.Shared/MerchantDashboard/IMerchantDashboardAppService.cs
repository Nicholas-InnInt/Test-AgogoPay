using Neptune.NsPay.MerchantDashboard.Dtos;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neptune.NsPay.MerchantDashboard
{
    public interface IMerchantDashboardAppService
    {
        Task<MerchantDashboardPageResultDto<MerchantDashboardDto>> GetAll(GetAllMerchantDashboardInput input);
        Task<IList<GetOrderMerchantViewDto>> GetOrderMerchants();
    }
}
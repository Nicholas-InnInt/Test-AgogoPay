using Abp.Application.Services;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.RechargeOrders.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RechargeOrders
{
    public interface IRechargeOrdersAppService: IApplicationService
    {
        Task<IList<GetOrderMerchantViewDto>> GetMerchants();

        Task CreateRecharge(CreateRechargeOrdersDto input);
    }
}

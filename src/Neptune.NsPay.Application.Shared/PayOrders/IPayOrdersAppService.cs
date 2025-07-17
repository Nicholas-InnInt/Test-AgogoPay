using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Common.Dto;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrders.Dtos;

namespace Neptune.NsPay.PayOrders
{
    public interface IPayOrdersAppService : IApplicationService
    {
        Task<PagedResultDto<GetPayOrderForViewDto>> GetAll(GetAllPayOrdersInput input);

        Task<GetPayOrderForViewDto> GetPayOrderForView(string id);

        Task<GetPayOrderForEditOutput> GetPayOrderForEdit(EntityDto<string> input);

        Task CallBcak(EntityDto<string> input);

        Task<ResultViewDto> EnforceCallBcak(EntityDto<string> input);
        Task AddMerchantBill(EntityDto<string> input);

        Task<IList<GetOrderMerchantViewDto>> GetOrderMerchants();
        bool IsShowMerchantFilter();

        Task<string> GetPayOrdersToExcel(GetAllPayOrdersForExcelInput input);

    }
}
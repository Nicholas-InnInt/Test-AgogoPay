using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrders.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neptune.NsPay.PayOrderDeposits
{
    public interface IPayOrderDepositsAppService : IApplicationService
    {
        Task<PagedResultDto<GetPayOrderDepositForViewDto>> GetAll(GetAllPayOrderDepositsInput input);

        Task<string> GetPayOrderDepositsToExcel(GetAllPayOrderDepositsForExcelInput input);

        Task<GetAssociatedDepositOrderOutput> GetAssociatedOrder(EntityDto<string> input, PayMentTypeEnum payType);

        Task<GetRejectDepositOrderOutput> GetRejectOrder(EntityDto<string> input, PayMentTypeEnum payType);

        Task RejectOrder(RejectPayOrderDepositDto input);

        Task AssociatedOrder(AssociatedDepositOrderCallBackDto input);

        Task AssociatedCryptoOrder(AssociatedDepositOrderCallBackDto input);

        Task<IList<GetOrderMerchantViewDto>> GetOrderMerchants();

        Task<GetPayOrderDepositForViewDto> GetPayOrderDepositForView(string id);

        bool IsShowMerchantFilter();

        Task BulkRejectOrder(BulkRejectPayOrderDepositDto input);
    }
}
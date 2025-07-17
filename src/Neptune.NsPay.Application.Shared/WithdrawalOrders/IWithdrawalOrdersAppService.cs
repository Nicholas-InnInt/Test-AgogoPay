using Abp.Application.Services.Dto;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.WithdrawalDevices.Dtos;
using Neptune.NsPay.WithdrawalOrders.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neptune.NsPay.WithdrawalOrders
{
    public interface IWithdrawalOrdersAppService
    {
        Task<WithdrawalOrderPageResultDto<GetWithdrawalOrderForViewDto>> GetAll(GetAllWithdrawalOrdersInput input);

        Task<GetWithdrawalOrderForViewDto> GetWithdrawalOrderForView(string id);
        Task<List<GetWithdrawalOrderForViewDto>> GetWithdrawalOrderListForView(List<string> ids);

        Task<GetDisplayProofDto> GetDisplayProofForView(string id);
        Task<GetWithdrawalOrderForViewDto> GetWithdrawalOrderForViewPayoutDetails(string id, string utcTimeFilter);
        Task<GetWithdrawalOrderForEditOutput> GetWithdrawalOrderForEdit(EntityDto<string> input);

        Task<string> GetWithdrawalOrdersToExcel(GetAllWithdrawalOrdersForExcelInput input);

        Task<IList<GetOrderMerchantViewDto>> GetOrderMerchants();
        bool IsShowDeviceFilter();
        Task<bool> EnforceCallBcak(EntityDto<string> input);

        Task<decimal> GetOrderMerchantBalance(string merchantCode);
        Task<bool> CallBcak(EntityDto<string> input);
        Task<bool> CallBackCancelOrder(EntityDto<string> input);

        Task<List<WithdrawalDeviceDto>> GetWithdrawalDeviceByMerchantCode(string merchantCode);
        Task<WithdrawalDeviceResultDto> UpdateWithdrawalOrderDevice(EditWithdrawalOrderDeviceDto input);
        Task<bool> ChangeOrderStatusToPending(EntityDto<string> input);

        Task<bool> ReleaseLockAmount(EntityDto<string> input);
    }
}

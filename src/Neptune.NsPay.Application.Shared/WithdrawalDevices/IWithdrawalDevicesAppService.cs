using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.WithdrawalDevices.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neptune.NsPay.WithdrawalDevices
{
    public interface IWithdrawalDevicesAppService : IApplicationService
    {
        Task<PagedResultDto<GetWithdrawalDeviceForViewDto>> GetAll(GetAllWithdrawalDevicesInput input);

        Task<GetWithdrawalDeviceForViewDto> GetWithdrawalDeviceForView(int id);

        Task<GetWithdrawalDeviceForEditOutput> GetWithdrawalDeviceForEdit(EntityDto input);

        Task<Dictionary<WithdrawalDevicesBankTypeEnum, List<WithdrawalDeviceDto>>> GetWithdrawalDeviceActiveBankList();

        Task CreateOrEdit(CreateOrEditWithdrawalDeviceDto input);

        Task Delete(EntityDto input);

        Task<List<MerchantDto>> GetMerchants();

        Task<bool> GetIsInternalMerchant();

    }
}

using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.MerchantSettings.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.MerchantSettings
{
    public interface IMerchantSettingsAppService : IApplicationService
    {
        Task<PagedResultDto<GetMerchantSettingForViewDto>> GetAll(GetAllMerchantSettingsInput input);

        Task<GetMerchantSettingForViewDto> GetMerchantSettingForView(int id);

        Task<GetMerchantSettingForEditOutput> GetMerchantSettingForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditMerchantSettingDto input);

        Task Delete(EntityDto input);

    }
}
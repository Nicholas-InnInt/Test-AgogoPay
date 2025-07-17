using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.NsPaySystemSettings.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.NsPaySystemSettings
{
    public interface INsPaySystemSettingsAppService : IApplicationService
    {
        Task<PagedResultDto<GetNsPaySystemSettingForViewDto>> GetAll(GetAllNsPaySystemSettingsInput input);

        Task<GetNsPaySystemSettingForViewDto> GetNsPaySystemSettingForView(int id);

        Task<GetNsPaySystemSettingForEditOutput> GetNsPaySystemSettingForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditNsPaySystemSettingDto input);

        Task Delete(EntityDto input);

    }
}
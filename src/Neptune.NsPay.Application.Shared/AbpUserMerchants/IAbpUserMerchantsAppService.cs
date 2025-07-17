using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.AbpUserMerchants.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.AbpUserMerchants
{
    public interface IAbpUserMerchantsAppService : IApplicationService
    {
        Task<PagedResultDto<GetAbpUserMerchantForViewDto>> GetAll(GetAllAbpUserMerchantsInput input);

        Task<GetAbpUserMerchantForViewDto> GetAbpUserMerchantForView(int id);

        Task<GetAbpUserMerchantForEditOutput> GetAbpUserMerchantForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditAbpUserMerchantDto input);

        Task Delete(EntityDto input);

    }
}
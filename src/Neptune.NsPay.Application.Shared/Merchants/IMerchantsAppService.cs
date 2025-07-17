using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.Dto;
using Neptune.NsPay.PayGroups.Dtos;
using System.Collections.Generic;

namespace Neptune.NsPay.Merchants
{
    public interface IMerchantsAppService : IApplicationService
    {
        Task<PagedResultDto<GetMerchantForViewDto>> GetAll(GetAllMerchantsInput input);

        Task<GetMerchantForViewDto> GetMerchantForView(int id);

        Task<List<PayGroupDto>> GetPayGroups();

		Task<GetMerchantForEditOutput> GetMerchantForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditMerchantDto input);

        Task Delete(EntityDto input);

    }
}
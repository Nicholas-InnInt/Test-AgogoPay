using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.MerchantRates.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.MerchantRates
{
    public interface IMerchantRatesAppService : IApplicationService
    {
        Task<PagedResultDto<GetMerchantRateForViewDto>> GetAll(GetAllMerchantRatesInput input);

        Task<GetMerchantRateForViewDto> GetMerchantRateForView(int id);

        Task<GetMerchantRateForEditOutput> GetMerchantRateForEdit(EntityDto input);

        Task<GetMerchantRateForEditOutput> GetMerchantRateByMerchantId(int merchantId);


		Task CreateOrEdit(CreateOrEditMerchantRateDto input);

        Task Delete(EntityDto input);

    }
}
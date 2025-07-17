using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.MerchantFunds.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.MerchantFunds
{
    public interface IMerchantFundsAppService : IApplicationService
    {
        Task<PagedResultDto<GetMerchantFundForViewDto>> GetAll(GetAllMerchantFundsInput input);

        Task<GetMerchantFundForViewDto> GetMerchantFundForView(int id);

        Task<GetMerchantFundForEditOutput> GetMerchantFundForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditMerchantFundDto input);

        Task Delete(EntityDto input);

    }
}
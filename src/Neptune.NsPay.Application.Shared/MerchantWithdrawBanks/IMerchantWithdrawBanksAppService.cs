using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.MerchantWithdrawBanks.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.MerchantWithdrawBanks
{
    public interface IMerchantWithdrawBanksAppService : IApplicationService
    {
        Task<PagedResultDto<GetMerchantWithdrawBankForViewDto>> GetAll(GetAllMerchantWithdrawBanksInput input);

        Task<GetMerchantWithdrawBankForViewDto> GetMerchantWithdrawBankForView(int id);

        Task<GetMerchantWithdrawBankForEditOutput> GetMerchantWithdrawBankForEdit(EntityDto input);

        Task<bool> CreateOrEdit(CreateOrEditMerchantWithdrawBankDto input);

        Task Delete(EntityDto input);

    }
}
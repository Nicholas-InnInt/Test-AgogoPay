using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.MerchantWithdraws.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.MerchantWithdraws
{
    public interface IMerchantWithdrawsAppService : IApplicationService
    {
        Task<PagedResultDto<GetMerchantWithdrawForViewDto>> GetAll(GetAllMerchantWithdrawsInput input);

        Task<GetMerchantWithdrawForViewDto> GetMerchantWithdrawForView(long id);

        Task<GetMerchantWithdrawForTurndownOutput> GetMerchantWithdrawForTurndown(EntityDto<long> input);

        Task<GetMerchantWithdrawForEditOutput> GetMerchantWithdrawForEdit(EntityDto<long> input);

        Task CreateOrEdit(CreateOrEditMerchantWithdrawDto input);

        Task<GetMerchantWithdrawForEditOutput> GetMerchantWithdrawForCreate();

        Task AuditTurndownOrPass(TurndownOrPassMerchantWithdrawDto input);

        Task AuditPass(EntityDto<long> input);

        Task<FileDto> GetMerchantWithdrawsToExcel(GetAllMerchantWithdrawsForExcelInput input);

    }
}
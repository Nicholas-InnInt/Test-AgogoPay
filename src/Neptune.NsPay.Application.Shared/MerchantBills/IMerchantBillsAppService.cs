using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Merchants.Dtos;
using System.Collections.Generic;

namespace Neptune.NsPay.MerchantBills
{
    public interface IMerchantBillsAppService : IApplicationService
    {
        Task<PagedResultDto<GetMerchantBillForViewDto>> GetAll(GetAllMerchantBillsInput input);

        Task<GetMerchantBillForViewDto> GetMerchantBillForView(string id);

        //Task<GetMerchantBillForEditOutput> GetMerchantBillForEdit(EntityDto<string> input);

        //Task CreateOrEdit(CreateOrEditMerchantBillDto input);


		Task<string> GetMerchantBillsToExcel(GetAllMerchantBillsForExcelInput input);

        bool IsShowMerchantFilter();
        Task<List<MerchantDto>> GetMerchants();


	}
}
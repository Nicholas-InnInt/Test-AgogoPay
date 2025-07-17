using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.PayGroups.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.PayGroups
{
    public interface IPayGroupsAppService : IApplicationService
    {
        Task<PagedResultDto<GetPayGroupForViewDto>> GetAll(GetAllPayGroupsInput input);

        Task<GetPayGroupForViewDto> GetPayGroupForView(int id);

        Task<GetPayGroupForEditOutput> GetPayGroupForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditPayGroupDto input);

        Task Delete(EntityDto input);

    }
}
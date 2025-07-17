using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.PayGroupMents.Dtos;
using Neptune.NsPay.Dto;
using Neptune.NsPay.PayMents.Dtos;
using System.Collections.Generic;

namespace Neptune.NsPay.PayGroupMents
{
    public interface IPayGroupMentsAppService : IApplicationService
    {
        Task<PagedResultDto<GetPayGroupMentForViewDto>> GetAll(GetAllPayGroupMentsInput input);

        Task<GetPayGroupMentForViewDto> GetPayGroupMentForView(int id);

        Task<List<CreateOrEditPayMentDto>> GetAllPayMentsByCreate();


		Task<GetPayGroupMentForEditOutput> GetPayGroupMentForEdit(int payGroupId);

        Task CreateOrEdit(CreateOrEditPayGroupMentDto input);

        Task Delete(EntityDto input);
        Task DeleteByPayGroup(int payGroupId);

    }
}
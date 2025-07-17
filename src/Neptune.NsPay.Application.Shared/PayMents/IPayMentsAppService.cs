using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.Dto;

namespace Neptune.NsPay.PayMents
{
    public interface IPayMentsAppService : IApplicationService
    {
        Task<PagedResultDto<GetPayMentForViewDto>> GetAll(GetAllPayMentsInput input);

        Task<GetPayMentForViewDto> GetPayMentForView(int id);

        Task<GetPayMentForEditOutput> GetPayMentForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditPayMentDto input);

        Task Delete(EntityDto input);
        Task GetHistory(EntityDto input);

    }
}
using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Neptune.NsPay.PayOrders.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay
{
    public interface IPublicPullAppService : IApplicationService
    {
        Task<PagedResultDto<GetPayOrderForViewDto>> GetAll(GetAllPayOrdersInput input);
    }
}

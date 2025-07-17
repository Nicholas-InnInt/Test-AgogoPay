using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.PayMents;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayMents.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_PayMents)]
    public class PayMentsController : NsPayControllerBase
    {
        private readonly IPayMentsAppService _payMentsAppService;

        public PayMentsController(IPayMentsAppService payMentsAppService)
        {
            _payMentsAppService = payMentsAppService;

        }

        public ActionResult Index()
        {
            var model = new PayMentsViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PayMents_Create, AppPermissions.Pages_PayMents_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            GetPayMentForEditOutput getPayMentForEditOutput;

            if (id.HasValue)
            {
                getPayMentForEditOutput = await _payMentsAppService.GetPayMentForEdit(new EntityDto { Id = (int)id });
            }
            else
            {
                getPayMentForEditOutput = new GetPayMentForEditOutput
                {
                    PayMent = new CreateOrEditPayMentDto()
                };
            }

            var viewModel = new CreateOrEditPayMentModalViewModel()
            {
                PayMent = getPayMentForEditOutput.PayMent,

            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PayMents_ChildEdit)]
        public async Task<PartialViewResult> EditPayMentModal(int? id)
        {
            GetPayMentForEditOutput getPayMentForEditOutput;

            if (id.HasValue)
            {
                getPayMentForEditOutput = await _payMentsAppService.GetPayMentForEdit(new EntityDto { Id = (int)id });
            }
            else
            {
                getPayMentForEditOutput = new GetPayMentForEditOutput
                {
                    PayMent = new CreateOrEditPayMentDto()
                };
            }

            var viewModel = new CreateOrEditPayMentModalViewModel()
            {
                PayMent = getPayMentForEditOutput.PayMent,

            };

            return PartialView("_EditPayMentModal", viewModel);
        }

        public async Task<PartialViewResult> ViewPayMentModal(int id)
        {
            var getPayMentForViewDto = await _payMentsAppService.GetPayMentForView(id);

            var model = new PayMentViewModel()
            {
                PayMent = getPayMentForViewDto.PayMent
            };

            return PartialView("_ViewPayMentModal", model);
        }

    }
}
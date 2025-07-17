using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.PayGroupMents;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.PayGroupMents.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_PayGroups)]
    public class PayGroupMentsController : NsPayControllerBase
    {
        private readonly IPayGroupMentsAppService _payGroupMentsAppService;

        public PayGroupMentsController(IPayGroupMentsAppService payGroupMentsAppService)
        {
            _payGroupMentsAppService = payGroupMentsAppService;

        }

        public ActionResult Index()
        {
            var model = new PayGroupMentsViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PayGroups_Create, AppPermissions.Pages_PayGroups_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? GroupId)
        {
            GetPayGroupMentForEditOutput getPayGroupMentForEditOutput;

            if (GroupId.HasValue)
            {
                getPayGroupMentForEditOutput = await _payGroupMentsAppService.GetPayGroupMentForEdit(GroupId.Value);
            }
            else
            {
                getPayGroupMentForEditOutput = new GetPayGroupMentForEditOutput
                {
                    PayGroupMent = new CreateOrEditPayGroupMentDto()
                };
            }

            var viewModel = new CreateOrEditPayGroupMentModalViewModel()
            {
                PayGroupMent = getPayGroupMentForEditOutput.PayGroupMent,
				PayMents = await _payGroupMentsAppService.GetAllPayMentsByCreate()
			};

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewPayGroupMentModal(int id)
        {
            var getPayGroupMentForViewDto = await _payGroupMentsAppService.GetPayGroupMentForView(id);

            var model = new PayGroupMentViewModel()
            {
                PayGroupMent = getPayGroupMentForViewDto.PayGroupMent
            };

            return PartialView("_ViewPayGroupMentModal", model);
        }

    }
}
using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.PayGroups;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.PayGroups.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_PayGroups)]
    public class PayGroupsController : NsPayControllerBase
    {
        private readonly IPayGroupsAppService _payGroupsAppService;

        public PayGroupsController(IPayGroupsAppService payGroupsAppService)
        {
            _payGroupsAppService = payGroupsAppService;

        }

        public ActionResult Index()
        {
            var model = new PayGroupsViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_PayGroups_Create, AppPermissions.Pages_PayGroups_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            GetPayGroupForEditOutput getPayGroupForEditOutput;

            if (id.HasValue)
            {
                getPayGroupForEditOutput = await _payGroupsAppService.GetPayGroupForEdit(new EntityDto { Id = (int)id });
            }
            else
            {
                getPayGroupForEditOutput = new GetPayGroupForEditOutput
                {
                    PayGroup = new CreateOrEditPayGroupDto()
                };
            }

            var viewModel = new CreateOrEditPayGroupModalViewModel()
            {
                PayGroup = getPayGroupForEditOutput.PayGroup
			};

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewPayGroupModal(int id)
        {
            var getPayGroupForViewDto = await _payGroupsAppService.GetPayGroupForView(id);

            var model = new PayGroupViewModel()
            {
                PayGroup = getPayGroupForViewDto.PayGroup
            };

            return PartialView("_ViewPayGroupModal", model);
        }

    }
}
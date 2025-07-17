using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.NsPayBackgroundJobs;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.NsPayBackgroundJobs;
using Neptune.NsPay.NsPayBackgroundJobs.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_NsPayBackgroundJobs)]
    public class NsPayBackgroundJobsController : NsPayControllerBase
    {
        private readonly INsPayBackgroundJobsAppService _nsPayBackgroundJobsAppService;

        public NsPayBackgroundJobsController(INsPayBackgroundJobsAppService nsPayBackgroundJobsAppService)
        {
            _nsPayBackgroundJobsAppService = nsPayBackgroundJobsAppService;

        }

        public ActionResult Index()
        {
            var model = new NsPayBackgroundJobsViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_NsPayBackgroundJobs_Create, AppPermissions.Pages_NsPayBackgroundJobs_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(Guid? id)
        {
            GetNsPayBackgroundJobForEditOutput getNsPayBackgroundJobForEditOutput;

            if (id.HasValue)
            {
                getNsPayBackgroundJobForEditOutput = await _nsPayBackgroundJobsAppService.GetNsPayBackgroundJobForEdit(new EntityDto<Guid> { Id = (Guid)id });
            }
            else
            {
                getNsPayBackgroundJobForEditOutput = new GetNsPayBackgroundJobForEditOutput
                {
                    NsPayBackgroundJob = new CreateOrEditNsPayBackgroundJobDto()
                };
            }

            var viewModel = new CreateOrEditNsPayBackgroundJobModalViewModel()
            {
                NsPayBackgroundJob = getNsPayBackgroundJobForEditOutput.NsPayBackgroundJob,

            };

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewNsPayBackgroundJobModal(Guid id)
        {
            var getNsPayBackgroundJobForViewDto = await _nsPayBackgroundJobsAppService.GetNsPayBackgroundJobForView(id);

            var model = new NsPayBackgroundJobViewModel()
            {
                NsPayBackgroundJob = getNsPayBackgroundJobForViewDto.NsPayBackgroundJob
            };

            return PartialView("_ViewNsPayBackgroundJobModal", model);
        }

    }
}
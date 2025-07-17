using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Areas.AppArea.Models.Merchants;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.Merchants.Dtos;
using Abp.Application.Services.Dto;
using Abp.Extensions;
using Neptune.NsPay.MerchantRates;
using Neptune.NsPay.MerchantRates.Dtos;
using Neptune.NsPay.RedisExtensions;
using GraphQL;
using Neptune.NsPay.Commons;
using System.Linq;
using Neptune.NsPay.PayGroups;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_Merchants)]
    public class MerchantsController : NsPayControllerBase
    {
        private readonly IMerchantsAppService _merchantsAppService;
		private readonly IMerchantRatesAppService _merchantRatesAppService;
		private readonly IRedisService _redisService;

        public MerchantsController(IMerchantsAppService merchantsAppService,
            IMerchantRatesAppService merchantRatesAppService,
            IRedisService redisService)
        {
            _merchantsAppService = merchantsAppService;
            _merchantRatesAppService = merchantRatesAppService;
            _redisService = redisService;
        }

        public ActionResult Index()
        {
            var model = new MerchantsViewModel
            {
                FilterText = ""
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Merchants_Create, AppPermissions.Pages_Merchants_Edit)]
        public async Task<PartialViewResult> CreateOrEditModal(int? id)
        {
            GetMerchantForEditOutput getMerchantForEditOutput;
            GetMerchantRateForEditOutput createOrEditMerchantRateDto = new GetMerchantRateForEditOutput()
            {
                MerchantRate = new CreateOrEditMerchantRateDto()
            };

            if (id.HasValue)
            {
                getMerchantForEditOutput = await _merchantsAppService.GetMerchantForEdit(new EntityDto { Id = (int)id });
                createOrEditMerchantRateDto = await _merchantRatesAppService.GetMerchantRateByMerchantId((int)id);
            }
            else
            {
                getMerchantForEditOutput = new GetMerchantForEditOutput
                {
                    Merchant = new CreateOrEditMerchantDto()
				};
			}
            var platform = _redisService.GetRedisValue<string>(NsPayRedisKeyConst.NsPaySystemKey + NsPaySystemSettingKeyConst.PlatformCode);
            var platformCodes = platform?.Split(',').ToList();

			var country = _redisService.GetRedisValue<string>(NsPayRedisKeyConst.NsPaySystemKey + NsPaySystemSettingKeyConst.Countries);
            var Countries = country.Split(',').ToList();

            var payGroups = await _merchantsAppService.GetPayGroups();

			var viewModel = new CreateOrEditMerchantModalViewModel()
            {
                Merchant = getMerchantForEditOutput.Merchant,
				MerchantRate= createOrEditMerchantRateDto.MerchantRate,
                PlatformCode= platformCodes,
				Countries = Countries,
				PayGroups= payGroups,
			};

            return PartialView("_CreateOrEditModal", viewModel);
        }

        public async Task<PartialViewResult> ViewMerchantModal(int id)
        {
            var getMerchantForViewDto = await _merchantsAppService.GetMerchantForView(id);

            var model = new MerchantViewModel()
            {
                Merchant = getMerchantForViewDto.Merchant
            };

            return PartialView("_ViewMerchantModal", model);
        }

    }
}
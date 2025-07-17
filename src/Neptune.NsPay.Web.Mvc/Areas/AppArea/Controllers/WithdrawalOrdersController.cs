using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.BankInfo;
using Neptune.NsPay.Commons;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Utils;
using Neptune.NsPay.VietQR;
using Neptune.NsPay.Web.Areas.AppArea.Models.WithdrawalOrders;
using Neptune.NsPay.Web.Controllers;
using Neptune.NsPay.WithdrawalOrders;
using Neptune.NsPay.WithdrawalOrders.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize(AppPermissions.Pages_WithdrawalOrders)]
    public class WithdrawalOrdersController : NsPayControllerBase
    {
        private readonly IWithdrawalOrdersAppService _withdrawalOrdersAppService;
        private readonly IRedisService _redisService;

        public WithdrawalOrdersController(IWithdrawalOrdersAppService withdrawalOrdersAppService,
            IRedisService redisService)
        {
            _withdrawalOrdersAppService = withdrawalOrdersAppService;
            _redisService = redisService;
        }

        public ActionResult Index()
        {
            var model = new WithdrawalOrdersViewModel
            {
                FilterText = "",
                IsShowDevice = _withdrawalOrdersAppService.IsShowDeviceFilter()
            };

            return View(model);
        }

        //[AbpMvcAuthorize(AppPermissions.Pages_WithdrawalOrders_Edit)]
        //public async Task<PartialViewResult> CreateOrEditModal(string id)
        //{
        //    GetWithdrawalOrderForEditOutput getWithdrawalOrderForEditOutput;

        //    if (!id.IsNullOrEmpty())
        //    {
        //        getWithdrawalOrderForEditOutput = await _withdrawalOrdersAppService.GetWithdrawalOrderForEdit(new EntityDto<string> { Id = id });
        //    }
        //    else
        //    {
        //        getWithdrawalOrderForEditOutput = new GetWithdrawalOrderForEditOutput
        //        {
        //            WithdrawalOrder = new CreateOrEditWithdrawalOrderDto()
        //        };
        //    }

        //    var viewModel = new CreateOrEditWithdrawalOrderModalViewModel()
        //    {
        //        WithdrawalOrder = getWithdrawalOrderForEditOutput.WithdrawalOrder,

        //    };

        //    return PartialView("_CreateOrEditModal", viewModel);
        //}

        public async Task<PartialViewResult> ViewWithdrawalOrderModal(string id)
        {
            var getWithdrawalOrderForViewDto = await _withdrawalOrdersAppService.GetWithdrawalOrderForView(id);

            var model = new WithdrawalOrderViewModel()
            {
                WithdrawalOrder = getWithdrawalOrderForViewDto.WithdrawalOrder
            };

            return PartialView("_ViewWithdrawalOrderModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_WithdrawalOrders_ChangeDevice)]
        public async Task<PartialViewResult> EditWithdrawalOrderDeviceModal(List<WithdrawalOrderModel> input)
        {
            string merchantCode = input.FirstOrDefault()?.MerchantCode;
            string merchantName = input.FirstOrDefault()?.MerchantName;

            //判断商户是否在配置中内部出款商户，如果不是默认NsPay
            var nspaySetting = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.InternalWithdrawMerchant);
            if (nspaySetting != null)
            {
                if (input.All(item => !nspaySetting.Contains(merchantCode)))
                {
                    merchantCode = NsPayRedisKeyConst.NsPay;
                }
            }


            var editModel = new EditWithdrawalOrderDeviceDto()
            {
                MerchantCode = merchantCode,
                WithdrawalIds = input.Select(p => p.WithdrawId).ToList(),
                OrderNos = input.Select(p => p.OrderNo).ToList(),
            };

            var model = new EditWithdrawalOrderDeviceModel()
            {
                WithdrawalDevice = await _withdrawalOrdersAppService.GetWithdrawalDeviceByMerchantCode(merchantCode),
                EditWithdrawalOrderDeviceDto = editModel,
                MerchantName = merchantName
            };

            return PartialView("_EditWithdrawalOrderDeviceModal", model);
        }


        [AbpMvcAuthorize(AppPermissions.Pages_WithdrawalOrders_Cancel)]
        public async Task<PartialViewResult> CancelWithdrawalOrderDeviceModal(List<WithdrawalOrderModel> input)
        {
            string merchantCode = input.FirstOrDefault()?.MerchantCode;
            string merchantName = input.FirstOrDefault()?.MerchantName;

            //判断商户是否在配置中内部出款商户，如果不是默认NsPay
            var nspaySetting = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.InternalWithdrawMerchant);
            if (nspaySetting != null)
            {
                if (input.All(item => !nspaySetting.Contains(merchantCode)))
                {
                    merchantCode = NsPayRedisKeyConst.NsPay;
                }
            }


            var editModel = new EditWithdrawalOrderDeviceDto()
            {
                MerchantCode = merchantCode,
                WithdrawalIds = input.Select(p => p.WithdrawId).ToList(),
                OrderNos = input.Select(p => p.OrderNo).ToList(),
            };

            var model = new EditWithdrawalOrderDeviceModel()
            {
                WithdrawalDevice = await _withdrawalOrdersAppService.GetWithdrawalDeviceByMerchantCode(merchantCode),
                EditWithdrawalOrderDeviceDto = editModel,
                MerchantName = merchantName
            };

            return PartialView("_CancelWithdrawalOrderDeviceModal", model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_WithdrawalOrders_ViewPayoutDetails)]
        public async Task<PartialViewResult> ViewPayoutDetailsModal(string id, string utcFilter)
        {
            var getWithdrawalOrderForViewDto = await _withdrawalOrdersAppService.GetWithdrawalOrderForViewPayoutDetails(id, utcFilter);

            var model = new WithdrawalOrderViewPayoutDetailsModel()
            {
                WithdrawalOrder = getWithdrawalOrderForViewDto.WithdrawalOrder,
                MerchantName = getWithdrawalOrderForViewDto.MerchantName,
            };

            var vietNameInEnglish = UtilsHelper.VietnameseToEnglish(model.WithdrawalOrder.BenAccountName);

            model.vietQRURL = "https://img.vietqr.io/image/" + BankApp.BanksObject[WithdrawalOrderBankMapper.FindBankByName(model.WithdrawalOrder.BenBankName)].bin + "-" + model.WithdrawalOrder.BenAccountNo + "-qr_only.png?amount=" + ((int)Convert.ToDecimal(model.WithdrawalOrder.OrderMoney)).ToString() + "&accountName=" + HttpUtility.UrlEncode(vietNameInEnglish)+"&addInfo=" + model.WithdrawalOrder.OrderNo;

            return PartialView("_ViewPayoutDetailsModal", model);
        }

        public async Task<PartialViewResult> ViewProofModal(string id)
        {
            var getWithdrawalOrderForViewDto = await _withdrawalOrdersAppService.GetDisplayProofForView(id);

            var model = new WithdrawalOrderDisplayProofModel()
            {
                Content = getWithdrawalOrderForViewDto,
                OrderId = id,
                OrderNumber = getWithdrawalOrderForViewDto.OrderNumber
            };

            return PartialView("_ViewProofModal", model);
        }

        public async Task<PartialViewResult> BatchCallBackWithdrawalModal(List<WithdrawalOrderModel> input)
        {
            var withdrawalIds = input.Select(p => p.WithdrawId).ToList();
            var withdrawalOrderForViews = await _withdrawalOrdersAppService.GetWithdrawalOrderListForView(withdrawalIds);

            var model = new Dictionary<string, List<WithdrawalOrderDto>>
            {
                { "Enforce", new() },
                { "Retry", new() },
                { "Cancel", new() }
            };
            foreach (var withdrawalOrderForView in withdrawalOrderForViews)
            {

                if (withdrawalOrderForView.WithdrawalOrder.OrderStatus != WithdrawalOrderStatusEnum.Success && withdrawalOrderForView.WithdrawalOrder.IsShowSuccessCallBack)
                {
                    model["Enforce"].Add(withdrawalOrderForView.WithdrawalOrder);
                }

                if (withdrawalOrderForView.WithdrawalOrder.OrderStatus == WithdrawalOrderStatusEnum.Success && withdrawalOrderForView.WithdrawalOrder.NotifyStatus == WithdrawalNotifyStatusEnum.Fail)
                {
                    model["Retry"].Add(withdrawalOrderForView.WithdrawalOrder);
                }

                if (withdrawalOrderForView.WithdrawalOrder.OrderStatus >= WithdrawalOrderStatusEnum.Fail)
                {
                    model["Cancel"].Add(withdrawalOrderForView.WithdrawalOrder);
                }
            }

            return PartialView("_BatchCallBackWithdrawalModal", model);
        }
    }
}
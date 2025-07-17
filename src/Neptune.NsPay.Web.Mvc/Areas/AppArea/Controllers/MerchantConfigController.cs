using Abp.AspNetCore.Mvc.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Graphics;
using Neptune.NsPay.MerchantConfig;
using Neptune.NsPay.Storage;
using Neptune.NsPay.Web.Areas.AppArea.Models.MerchantConfig;
using Neptune.NsPay.Web.Controllers;
using System.Linq;
using System;
using System.Threading.Tasks;
using Abp.IO.Extensions;
using Abp.AspNetZeroCore.Net;
using Abp.Collections.Extensions;
using Abp.Authorization;
using System.Collections.Generic;
using Neptune.NsPay.MerchantConfig.Dto;
using Neptune.NsPay.MerchantWithdraws.Dtos;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
	[Area("AppArea")]
	[AbpMvcAuthorize(AppPermissions.Pages_MerchantConfig)]
	public class MerchantConfigController : NsPayControllerBase
	{
		private readonly IMerchantConfigAppService _merchantConfigAppService;
		private readonly IBinaryObjectManager _binaryObjectManager;
		private readonly IImageValidator _imageValidator;
		public MerchantConfigController(
			IBinaryObjectManager binaryObjectManager,
			IMerchantConfigAppService merchantConfigAppService,
			IImageValidator imageValidator)
		{
			_binaryObjectManager = binaryObjectManager;
			_merchantConfigAppService = merchantConfigAppService;
			_imageValidator = imageValidator;
		}

		public async Task<IActionResult> IndexAsync()
		{
            MerchantConfigViewModel model = new MerchantConfigViewModel();
            var settingViewDto = await _merchantConfigAppService.GetMerchantConfig();
            model.MerchantCode = settingViewDto.MerchantCode;
            model.MerchantId = settingViewDto.MerchantId;
            model.Title = settingViewDto.Title;
            model.OrderBankRemark = settingViewDto.OrderBankRemark;
            model.LogoUrl = settingViewDto.LogoUrl;
            model.LoginIpAddress = settingViewDto.LoginIpAddress;
            model.MerchantConfigBank = settingViewDto.MerchantConfigBank;
            model.BankNotifyText = settingViewDto.BankNotifyText;
            model.TelegramNotifyBotId = settingViewDto.TelegramNotifyBotId;
            model.TelegramNotifyChatId = settingViewDto.TelegramNotifyChatId;
            model.OpenRiskWithdrawal = settingViewDto.OpenRiskWithdrawal;
            model.PlatformUrl = settingViewDto.PlatformUrl;
            model.PlatformUserName = settingViewDto.PlatformUserName;
            model.PlatformPassWord = settingViewDto.PlatformPassWord;
            model.PlatformLimitMoney = settingViewDto.PlatformLimitMoney;
            model.MerchantBanks = await _merchantConfigAppService.GetMerchantBanks();
            return View(model);
		}

        [HttpPost]
        [AbpMvcAuthorize(AppPermissions.Pages_MerchantConfig_Edit)]
        public async Task<JsonResult> UploadMerchantLogo()
        {
            try
            {
                var logoObject = await UploadLogoFileInternal();

                var merchant = await _merchantConfigAppService.GetMerchantUserAsync(logoObject.id);

                merchant.LogoId = logoObject.id;
                merchant.LogoFileType = logoObject.contentType;

                return Json(new AjaxResponse(new
                { id = logoObject.id, MerchantId = merchant.MerchantId, fileType = merchant.LogoFileType }));
            }
            catch (UserFriendlyException ex)
            {
                return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
            }
        }

        private async Task<(Guid id, string contentType)> UploadLogoFileInternal()
        {
            var logoFile = Request.Form.Files.First();

            //Check input
            if (logoFile == null)
            {
                throw new UserFriendlyException(L("File_Empty_Error"));
            }

            if (logoFile.Length > 5242880) //30KB
            {
                throw new UserFriendlyException(L("File_SizeLimit_Error"));
            }

            byte[] fileBytes;
            await using (var stream = logoFile.OpenReadStream())
            {
                fileBytes = stream.GetAllBytes();
                _imageValidator.Validate(fileBytes);
            }

            var logoObject = new BinaryObject(AbpSession.GetTenantId(), fileBytes, $"Logo {DateTime.UtcNow}");
            await _binaryObjectManager.SaveAsync(logoObject);
            return (logoObject.Id, logoFile.ContentType);
        }

        public async Task<FileResult> GetProfilePicture()
        {
            var output = await _merchantConfigAppService.GetMerchantLogoPicture();

            if (output.ProfilePicture.IsNullOrEmpty())
            {
                return GetDefaultProfilePictureInternal();
            }

            return File(Convert.FromBase64String(output.ProfilePicture), MimeTypeNames.ImageJpeg);
        }

        protected FileResult GetDefaultProfilePictureInternal()
        {
            return File("", MimeTypeNames.ImagePng);
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Authorization.Permissions;
using Neptune.NsPay.Authorization.Permissions.Dto;
using Neptune.NsPay.Authorization.Roles;
using Neptune.NsPay.Authorization.Roles.Dto;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Authorization.Users.Importing;
using Neptune.NsPay.DataImporting.Excel;
using Neptune.NsPay.Security;
using Neptune.NsPay.Storage;
using Neptune.NsPay.Web.Areas.AppArea.Models.Users;
using Neptune.NsPay.Web.Controllers;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize]
    public class UsersController : ExcelImportControllerBase
    {
        private readonly IMerchantUserAppService _merchantUserAppService;
        private readonly IUserAppService _userAppService;
        private readonly UserManager _userManager;
        private readonly IRoleAppService _roleAppService;
        private readonly IPermissionAppService _permissionAppService;
        private readonly IPasswordComplexitySettingStore _passwordComplexitySettingStore;
        private readonly IOptions<UserOptions> _userOptions;
        public override string ImportExcelPermission => AppPermissions.Pages_Administration_Users_Create;

        public UsersController(
            IUserAppService userAppService,
            IMerchantUserAppService merchantUserAppService,
            UserManager userManager,
            IRoleAppService roleAppService,
            IPermissionAppService permissionAppService,
            IPasswordComplexitySettingStore passwordComplexitySettingStore,
            IOptions<UserOptions> userOptions,
            IBinaryObjectManager binaryObjectManager,
            IBackgroundJobManager backgroundJobManager
        ) : base(binaryObjectManager, backgroundJobManager)
        {
            _merchantUserAppService = merchantUserAppService;
            _userAppService = userAppService;
            _userManager = userManager;
            _roleAppService = roleAppService;
            _permissionAppService = permissionAppService;
            _passwordComplexitySettingStore = passwordComplexitySettingStore;
            _userOptions = userOptions;
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Administration_Users)]
        public async Task<ActionResult> Index()
        {
            var roles = new List<ComboboxItemDto>();

            if (await IsGrantedAsync(AppPermissions.Pages_Administration_Roles))
            {
                var getRolesOutput = await _roleAppService.GetRoles(new GetRolesInput());
                roles = getRolesOutput.Items.Select(r => new ComboboxItemDto(r.Id.ToString(), r.DisplayName)).ToList();
            }

            roles.Insert(0, new ComboboxItemDto("", L("FilterByRole")));

            var permissions = _permissionAppService.GetAllPermissions().Items.ToList();

            var model = new UsersViewModel
            {
                FilterText = Request.Query["filterText"],
                Roles = roles,
                Permissions = ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.DisplayName)
                    .ToList(),
                OnlyLockedUsers = false
            };

            return View(model);
        }

        [AbpMvcAuthorize(AppPermissions.Pages_Merchant_Users)]
        public async Task<ActionResult> Merchant()
        {
            var roles = new List<ComboboxItemDto>();

            var roleDtoList = await _merchantUserAppService.GetMerchantUserRolesList();

            if(roleDtoList!=null)
            {
                roles = roleDtoList.Select(r => new ComboboxItemDto(r.Id.ToString(), r.DisplayName)).ToList();
            }
         
            roles.Insert(0, new ComboboxItemDto("", L("FilterByRole")));

            var permissions = _permissionAppService.GetAllPermissions().Items.ToList();

            var model = new UsersViewModel
            {
                FilterText = Request.Query["filterText"],
                Roles = roles,
                Permissions = ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.DisplayName)
                    .ToList(),
                OnlyLockedUsers = false
            };

            return View(model);
        }

        [AbpMvcAuthorize(
            AppPermissions.Pages_Administration_Users,
            AppPermissions.Pages_Administration_Users_Create,
            AppPermissions.Pages_Administration_Users_Edit
        )]
        public async Task<PartialViewResult> CreateOrEditModal(long? id)
        {
            var output = await _userAppService.GetUserForEdit(new NullableIdDto<long> { Id = id });
            var viewModel = ObjectMapper.Map<CreateOrEditUserModalViewModel>(output);
            viewModel.PasswordComplexitySetting = await _passwordComplexitySettingStore.GetSettingsAsync();
            viewModel.AllowedUserNameCharacters = _userOptions.Value.AllowedUserNameCharacters;
            return PartialView("_CreateOrEditModal", viewModel);
        }

        [AbpMvcAuthorize(
           AppPermissions.Pages_Merchant_Users,
           AppPermissions.Pages_Merchant_Users_Create,
           AppPermissions.Pages_Merchant_Users_Edit
       )]
        public async Task<PartialViewResult> CreateOrEditModalMerchant(long? id)
        {
            var output = await _merchantUserAppService.GetMerchantUserForEdit(new NullableIdDto<long> { Id = id });
            var viewModel = ObjectMapper.Map<CreateOrEditUserModalViewModel>(output);
            viewModel.PasswordComplexitySetting = await _passwordComplexitySettingStore.GetSettingsAsync();
            viewModel.AllowedUserNameCharacters = _userOptions.Value.AllowedUserNameCharacters;
            return PartialView("_CreateOrEditModalMerchant", viewModel);
        }

        [AbpMvcAuthorize(
            AppPermissions.Pages_Administration_Users,
            AppPermissions.Pages_Administration_Users_ChangePermissions
        )]
        public async Task<PartialViewResult> PermissionsModal(long id)
        {
            var output = await _userAppService.GetUserPermissionsForEdit(new EntityDto<long>(id));
            var viewModel = ObjectMapper.Map<UserPermissionsEditViewModel>(output);
            viewModel.User = await _userManager.GetUserByIdAsync(id);
            ;
            return PartialView("_PermissionsModal", viewModel);
        }
        
        public async Task<PartialViewResult> ExcelExportModal(long? id)
        {
            var output = await _userAppService.GetUserExcelColumnsToExcel();
            var viewModel = new UserExportExcelViewModel
            {
                UserExcelColumns = output
            };
            
            return PartialView("_ExportExcelModal", viewModel);
        }

        public ActionResult LoginAttempts()
        {
            var loginResultTypes = Enum.GetNames(typeof(AbpLoginResultType))
                .Select(e => new ComboboxItemDto(e, L("AbpLoginResultType_" + e)))
                .ToList();

            loginResultTypes.Insert(0, new ComboboxItemDto("", L("All")));

            return View("LoginAttempts", new UserLoginAttemptsViewModel()
            {
                LoginAttemptResults = loginResultTypes
            });
        }

        public override async Task EnqueueExcelImportJobAsync(ImportFromExcelJobArgs args)
        {
            await BackgroundJobManager.EnqueueAsync<ImportUsersToExcelJob, ImportFromExcelJobArgs>(args);
        }
    }
}
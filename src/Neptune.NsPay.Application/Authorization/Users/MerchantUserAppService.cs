using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Organizations;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Zero.Configuration;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniExcelLibs;
using Neptune.NsPay.Authorization.Permissions;
using Neptune.NsPay.Authorization.Permissions.Dto;
using Neptune.NsPay.Authorization.Roles;
using Neptune.NsPay.Authorization.Users.Dto;
using Neptune.NsPay.Authorization.Users.Exporting;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Exporting;
using Neptune.NsPay.Net.Emailing;
using Neptune.NsPay.Notifications;
using Neptune.NsPay.Url;
using Neptune.NsPay.Organizations.Dto;
using Neptune.NsPay.AbpUserMerchants;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.Authorization.Roles.Dto;
using Neptune.NsPay.Commons;
using Abp.Collections.Extensions;
using Org.BouncyCastle.Crypto.Modes.Gcm;

namespace Neptune.NsPay.Authorization.Users
{

    [AbpAuthorize(AppPermissions.Pages_Merchant_Users)]
    public class MerchantUserAppService : NsPayAppServiceBase, IMerchantUserAppService
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly RoleManager _roleManager;
        private readonly IUserEmailer _userEmailer;
        private readonly IUserListExcelExporter _userListExcelExporter;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<RolePermissionSetting, long> _rolePermissionRepository;
        private readonly IRepository<UserPermissionSetting, long> _userPermissionRepository;
        private readonly IRepository<UserRole, long> _userRoleRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IUserPolicy _userPolicy;
        private readonly IEnumerable<IPasswordValidator<User>> _passwordValidators;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRepository<OrganizationUnit, long> _organizationUnitRepository;
        private readonly IRoleManagementConfig _roleManagementConfig;
        private readonly UserManager _userManager;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<OrganizationUnitRole, long> _organizationUnitRoleRepository;
        private readonly IOptions<UserOptions> _userOptions;
        private readonly IEmailSettingsChecker _emailSettingsChecker;
        private readonly IRepository<Merchant> _merchantRepository;
        private readonly IRepository<AbpUserMerchant> _abpUserMerchantRepository;
        private readonly IRedisService _redisService;

        public MerchantUserAppService(
            RoleManager roleManager,
            IUserEmailer userEmailer,
            IUserListExcelExporter userListExcelExporter,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IAppNotifier appNotifier,
            IRepository<RolePermissionSetting, long> rolePermissionRepository,
            IRepository<UserPermissionSetting, long> userPermissionRepository,
            IRepository<UserRole, long> userRoleRepository,
            IRepository<Role> roleRepository,
            IUserPolicy userPolicy,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            IPasswordHasher<User> passwordHasher,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRoleManagementConfig roleManagementConfig,
            UserManager userManager,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository,
            IOptions<UserOptions> userOptions, IEmailSettingsChecker emailSettingsChecker,
            IRepository<Merchant> merchantRepository,
            IRepository<AbpUserMerchant> abpUserMerchantRepository,
            IRedisService redisService)
        {
            _roleManager = roleManager;
            _userEmailer = userEmailer;
            _userListExcelExporter = userListExcelExporter;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _rolePermissionRepository = rolePermissionRepository;
            _userPermissionRepository = userPermissionRepository;
            _userRoleRepository = userRoleRepository;
            _userPolicy = userPolicy;
            _passwordValidators = passwordValidators;
            _passwordHasher = passwordHasher;
            _organizationUnitRepository = organizationUnitRepository;
            _roleManagementConfig = roleManagementConfig;
            _userManager = userManager;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _organizationUnitRoleRepository = organizationUnitRoleRepository;
            _userOptions = userOptions;
            _emailSettingsChecker = emailSettingsChecker;
            _roleRepository = roleRepository;
            _merchantRepository = merchantRepository;
            _abpUserMerchantRepository = abpUserMerchantRepository;
            _redisService = redisService;

            AppUrlService = NullAppUrlService.Instance;
        }

        [HttpPost]
        public async Task<PagedResultDto<UserListDto>> GetMerchantUsers(GetUsersInput input)
        {

            var user = await GetCurrentUserAsync();
            var userRoles = await GetMerchantUserRoles();

            var userRolesIdList = userRoles.Select(x => x.Id);
            var userMerchantId = user.Merchants.First().Id;
            // must belong login merchant and users roles only
            var merchantUser = _abpUserMerchantRepository.GetAll().Where(x=>x.MerchantId == userMerchantId);
            var userWithSameRoles = _userRoleRepository.GetAll().Where(x => userRolesIdList.Contains(x.RoleId)).Select(x=> x.UserId).Distinct();
            var selectedUser = merchantUser.Join(userWithSameRoles, t1 => t1.UserId, t2 => t2, (t1, t2) => t1);

            var query = UserManager.Users.Join(selectedUser,t1=>t1.Id , t2 => t2.UserId, (t1, t2) => t1)//
              .WhereIf(input.OnlyLockedUsers,
                  u => u.LockoutEndDateUtc.HasValue && u.LockoutEndDateUtc.Value > DateTime.UtcNow)
              .WhereIf(
                  !input.Filter.IsNullOrWhiteSpace(),
                  u =>
                      u.Name.Contains(input.Filter) ||
                      u.Surname.Contains(input.Filter) ||
                      u.UserName.Contains(input.Filter) ||
                      u.EmailAddress.Contains(input.Filter)
              );

            var userCount = await query.CountAsync();

            var users = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            var userListDtos = ObjectMapper.Map<List<UserListDto>>(users);
            await FillRoleNames(userListDtos);

            return new PagedResultDto<UserListDto>(
                userCount,
                userListDtos
            );
        }

        private async Task FillRoleNames(IReadOnlyCollection<UserListDto> userListDtos)
        {
            /* This method is optimized to fill role names to given list. */
            var userIds = userListDtos.Select(u => u.Id);

            var userRoles = await _userRoleRepository.GetAll()
                .Where(userRole => userIds.Contains(userRole.UserId))
                .Select(userRole => userRole).ToListAsync();

            var distinctRoleIds = userRoles.Select(userRole => userRole.RoleId).Distinct();

            foreach (var user in userListDtos)
            {
                var rolesOfUser = userRoles.Where(userRole => userRole.UserId == user.Id).ToList();
                user.Roles = ObjectMapper.Map<List<UserListRoleDto>>(rolesOfUser);
            }

            var roleNames = new Dictionary<int, string>();
            foreach (var roleId in distinctRoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role != null)
                {
                    roleNames[roleId] = role.DisplayName;
                }
            }

            foreach (var userListDto in userListDtos)
            {
                foreach (var userListRoleDto in userListDto.Roles)
                {
                    if (roleNames.ContainsKey(userListRoleDto.RoleId))
                    {
                        userListRoleDto.RoleName = roleNames[userListRoleDto.RoleId];
                    }
                }

                userListDto.Roles = userListDto.Roles.Where(r => r.RoleName != null).OrderBy(r => r.RoleName).ToList();
            }
        }


        public async Task<List<RoleListDto>> GetMerchantUserRolesList()
        {
            var returnData = new List<RoleListDto>();
            var rolesList = await GetMerchantUserRoles();
            returnData = ObjectMapper.Map<List<RoleListDto>>(rolesList);
            return returnData;
        }

        private async Task<List<Role>> GetMerchantUserRoles()
        {
            var returnData = new List<Role>();
            var merchantUserRolesValue = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.MerchantUserRoles);

            if (!merchantUserRolesValue.IsNullOrEmpty())
            {
                var rolesNameList = merchantUserRolesValue.Split(',');
                returnData = await _roleManager.Roles.Where(x => rolesNameList.Contains(x.DisplayName)).ToListAsync();
            }
            return returnData;
        }

        [AbpAuthorize(AppPermissions.Pages_Merchant_Users_Create, AppPermissions.Pages_Merchant_Users_Edit)]
        public async Task<GetUserForEditOutput> GetMerchantUserForEdit(NullableIdDto<long> input)
        {
            var loginUser = await GetCurrentUserAsync();
            var merchantUserRoles = await GetMerchantUserRoles();
            //Getting all available roles
            var userRoleDtos = merchantUserRoles
                .OrderBy(r => r.DisplayName)
                .Select(r => new UserRoleDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    RoleDisplayName = r.DisplayName
                })
                .ToArray();

            var merchantInfo = await _merchantRepository.FirstOrDefaultAsync(x => x.Id == loginUser.Merchants.First().Id);

            var userMerchantDtos = (new List<UserMerchantDto>() { new UserMerchantDto()
            {
                MerchantId = merchantInfo.Id,
                MerchantName = merchantInfo.Name,
                MerchantDisplayName = merchantInfo.Name,
            } }).ToArray();

            var allOrganizationUnits = await _organizationUnitRepository.GetAllListAsync();

            var output = new GetUserForEditOutput
            {
                Roles = userRoleDtos,
                Merechants = userMerchantDtos,
                AllOrganizationUnits = ObjectMapper.Map<List<OrganizationUnitDto>>(allOrganizationUnits),
                MemberedOrganizationUnits = new List<string>(),
                AllowedUserNameCharacters = _userOptions.Value.AllowedUserNameCharacters,
                IsSMTPSettingsProvided = await _emailSettingsChecker.EmailSettingsValidAsync()
            };

            if (!input.Id.HasValue)
            {
                // Creating a new user
                output.User = new UserEditDto
                {
                    IsActive = true,
                    ShouldChangePasswordOnNextLogin = true,
                    IsTwoFactorEnabled =
                        await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement
                            .TwoFactorLogin.IsEnabled),
                    IsLockoutEnabled =
                        await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.UserLockOut
                            .IsEnabled)
                };

                foreach (var defaultRole in await _roleManager.Roles.Where(r => r.IsDefault).ToListAsync())
                {
                    var defaultUserRole = userRoleDtos.FirstOrDefault(ur => ur.RoleName == defaultRole.Name);
                    if (defaultUserRole != null)
                    {
                        defaultUserRole.IsAssigned = true;
                    }
                }

                userMerchantDtos.First().IsAssigned = true;
            }
            else
            {
                //Editing an existing user
                var user = await UserManager.GetUserByIdAsync(input.Id.Value);

                output.User = ObjectMapper.Map<UserEditDto>(user);
                output.ProfilePictureId = user.ProfilePictureId;

                var organizationUnits = await UserManager.GetOrganizationUnitsAsync(user);
                output.MemberedOrganizationUnits = organizationUnits.Select(ou => ou.Code).ToList();

                var allRolesOfUsersOrganizationUnits = GetAllRoleNamesOfUsersOrganizationUnits(input.Id.Value);

                foreach (var userRoleDto in userRoleDtos)
                {
                    userRoleDto.IsAssigned = await UserManager.IsInRoleAsync(user, userRoleDto.RoleName);
                    userRoleDto.InheritedFromOrganizationUnit =
                        allRolesOfUsersOrganizationUnits.Contains(userRoleDto.RoleName);
                }

                var userMerchants = await _abpUserMerchantRepository.GetAllListAsync(r => r.UserId == user.Id);

                foreach (var userMerchantDto in userMerchantDtos)
                {
                    var defaultUserRole = userMerchants.FirstOrDefault(r => r.MerchantId == userMerchantDto.MerchantId);
                    if (defaultUserRole != null)
                    {
                        userMerchantDto.IsAssigned = true;
                    }
                }
            }

            output.SupprtedUserType = new List<UserTypeEnum>() { loginUser.UserType };

            return output;
        }


        public async Task CreateOrUpdateMerchantUser(CreateOrUpdateUserInput input)
        {
            var allowedMerchantUserRoles = await GetMerchantUserRoles();

            if (allowedMerchantUserRoles != null && !input.AssignedRoleNames.All(x => allowedMerchantUserRoles.Select(y => y.Name).Contains(x)))
            {
                throw new UserFriendlyException(L("InvalidMerchantUserRole"));
            }

            if (input.User.Id.HasValue)
            {
                await UpdateUserAsync(input);
            }
            else
            {
                await CreateUserAsync(input);
            }

        }


        [AbpAuthorize(AppPermissions.Pages_Merchant_Users_Delete)]
        public async Task DeleteMerchantUser(EntityDto<long> input)
        {
            if (input.Id == AbpSession.GetUserId())
            {
                throw new UserFriendlyException(L("YouCanNotDeleteOwnAccount"));
            }

            var user = await UserManager.GetUserByIdAsync(input.Id);
            CheckErrors(await UserManager.DeleteAsync(user));
        }


        [AbpAuthorize( AppPermissions.Pages_Merchant_Users_Edit)]
        protected virtual async Task UpdateUserAsync(CreateOrUpdateUserInput input)
        {
            Debug.Assert(input.User.Id != null, "input.User.Id should be set.");

            var user = await UserManager.FindByIdAsync(input.User.Id.Value.ToString());

            if (user is null)
            {
                throw new AbpException(L("UserNotFound"));
            }

            var isEmailChanged = user.EmailAddress != input.User.EmailAddress;

            if (isEmailChanged)
            {
                user.IsEmailConfirmed = false;
            }

            //Update user properties
            ObjectMapper.Map(input.User, user); //Passwords is not mapped (see mapping configuration)

            CheckErrors(await UserManager.UpdateAsync(user));

            if (input.SetRandomPassword)
            {
                var randomPassword = await _userManager.CreateRandomPassword();
                user.Password = _passwordHasher.HashPassword(user, randomPassword);
                input.User.Password = randomPassword;
            }
            else if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                CheckErrors(await UserManager.ChangePasswordAsync(user, input.User.Password));
            }

            //Update roles
            CheckErrors(await UserManager.SetRolesAsync(user, input.AssignedRoleNames));

            //修改关联商户
            await _abpUserMerchantRepository.DeleteAsync(r => r.UserId == user.Id);
            foreach (var merchantName in input.AssignedMerchantNames)
            {
                var merchant = await _merchantRepository.FirstOrDefaultAsync(r => r.Name == merchantName);
                AbpUserMerchant abpUserMerchant = new AbpUserMerchant()
                {
                    MerchantId = merchant.Id,
                    TenantId = AbpSession.TenantId,
                    UserId = user.Id
                };
                await _abpUserMerchantRepository.InsertAsync(abpUserMerchant);
            }

            //update organization units
            await UserManager.SetOrganizationUnitsAsync(user, input.OrganizationUnits.ToArray());

            if (input.SendActivationEmail || isEmailChanged)
            {
                user.SetNewEmailConfirmationCode();
                await _userEmailer.SendEmailActivationLinkAsync(
                    user,
                    AppUrlService.CreateEmailActivationUrlFormat(AbpSession.TenantId),
                    input.User.Password
                );
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Merchant_Users_Create)]
        protected virtual async Task CreateUserAsync(CreateOrUpdateUserInput input)
        {
            if (AbpSession.TenantId.HasValue)
            {
                await _userPolicy.CheckMaxUserCountAsync(AbpSession.GetTenantId());
            }

            var user = ObjectMapper.Map<User>(input.User); //Passwords is not mapped (see mapping configuration)
            user.TenantId = AbpSession.TenantId;

            //Set password
            if (input.SetRandomPassword)
            {
                var randomPassword = await _userManager.CreateRandomPassword();
                user.Password = _passwordHasher.HashPassword(user, randomPassword);
                input.User.Password = randomPassword;
            }
            else if (!input.User.Password.IsNullOrEmpty())
            {
                await UserManager.InitializeOptionsAsync(AbpSession.TenantId);
                foreach (var validator in _passwordValidators)
                {
                    CheckErrors(await validator.ValidateAsync(UserManager, user, input.User.Password));
                }

                user.Password = _passwordHasher.HashPassword(user, input.User.Password);
            }

            user.ShouldChangePasswordOnNextLogin = input.User.ShouldChangePasswordOnNextLogin;

            //Assign roles
            user.Roles = new Collection<UserRole>();
            foreach (var roleName in input.AssignedRoleNames)
            {
                var role = await _roleManager.GetRoleByNameAsync(roleName);
                user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, role.Id));
            }

            CheckErrors(await UserManager.CreateAsync(user));
            await CurrentUnitOfWork.SaveChangesAsync(); //To get new user's Id.

            //添加商户关联表
            foreach (var merchantName in input.AssignedMerchantNames)
            {
                var merchant = await _merchantRepository.FirstOrDefaultAsync(r => r.Name == merchantName);
                AbpUserMerchant abpUserMerchant = new AbpUserMerchant()
                {
                    MerchantId = merchant.Id,
                    TenantId = AbpSession.TenantId,
                    UserId = user.Id
                };
                await _abpUserMerchantRepository.InsertAsync(abpUserMerchant);
            }

            //Notifications
            await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());
            await _appNotifier.WelcomeToTheApplicationAsync(user);

            //Organization Units
            await UserManager.SetOrganizationUnitsAsync(user, input.OrganizationUnits.ToArray());

            //Send activation email
            if (input.SendActivationEmail)
            {
                user.SetNewEmailConfirmationCode();
                await _userEmailer.SendEmailActivationLinkAsync(
                    user,
                    AppUrlService.CreateEmailActivationUrlFormat(AbpSession.TenantId),
                    input.User.Password
                );
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Merchant_Users_Unlock)]
        public async Task UnlockUser(EntityDto<long> input)
        {
            var user = await UserManager.GetUserByIdAsync(input.Id);
            user.Unlock();
        }
        private List<string> GetAllRoleNamesOfUsersOrganizationUnits(long userId)
        {
            return (from userOu in _userOrganizationUnitRepository.GetAll()
                    join roleOu in _organizationUnitRoleRepository.GetAll() on userOu.OrganizationUnitId equals roleOu
                        .OrganizationUnitId
                    join userOuRoles in _roleRepository.GetAll() on roleOu.RoleId equals userOuRoles.Id
                    where userOu.UserId == userId
                    select userOuRoles.Name).ToList();
        }

    }
}

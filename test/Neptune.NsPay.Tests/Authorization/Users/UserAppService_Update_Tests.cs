﻿using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.UI;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neptune.NsPay.Authorization.Roles;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Authorization.Users.Dto;
using Shouldly;
using Xunit;

namespace Neptune.NsPay.Tests.Authorization.Users
{
    // ReSharper disable once InconsistentNaming
    public class UserAppService_Update_Tests : UserAppServiceTestBase
    {
        [Fact]
        public async Task Update_User_Basic_Tests()
        {
            //Arrange
            var managerRole = CreateRole("Manager");
            var adminUser = await GetUserByUserNameOrNullAsync(AbpUserBase.AdminUserName);

            //Act
            await UserAppService.CreateOrUpdateUser(
                new CreateOrUpdateUserInput
                {
                    User = new UserEditDto
                    {
                        Id = adminUser.Id,
                        EmailAddress = "admin1@abp.com",
                        Name = "System1",
                        Surname = "Admin2",
                        Password = "123qwE*",
                        UserName = adminUser.UserName
                    },
                    AssignedRoleNames = new[] { "Manager" }
                });

            //Assert
            await UsingDbContextAsync(async context =>
            {
                //Get created user
                var updatedAdminUser = await GetUserByUserNameOrNullAsync(adminUser.UserName, includeRoles: true);
                updatedAdminUser.ShouldNotBe(null);
                updatedAdminUser.Id.ShouldBe(adminUser.Id);

                //Check some properties
                updatedAdminUser.EmailAddress.ShouldBe("admin1@abp.com");
                updatedAdminUser.TenantId.ShouldBe(AbpSession.TenantId);

                LocalIocManager
                    .Resolve<IPasswordHasher<User>>()
                    .VerifyHashedPassword(updatedAdminUser, updatedAdminUser.Password, "123qwE*")
                    .ShouldBe(PasswordVerificationResult.Success);

                //Check roles
                updatedAdminUser.Roles.Count.ShouldBe(2); // Admin role is always assigned to admin user
                updatedAdminUser.Roles.Any(ur => ur.RoleId == managerRole.Id).ShouldBe(true);
            });
        }

        [Fact]
        public async Task Should_Not_Update_User_With_Duplicate_Username_Or_EmailAddress()
        {
            //Arrange

            CreateTestUsers();
            var jnashUser = await GetUserByUserNameOrNullAsync("jnash");

            //Act

            //Try to update with existing username
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                await UserAppService.CreateOrUpdateUser(
                    new CreateOrUpdateUserInput
                    {
                        User = new UserEditDto
                               {
                                   Id = jnashUser.Id,
                                   EmailAddress = "jnsh2000@testdomain.com",
                                   Name = "John",
                                   Surname = "Nash",
                                   UserName = "adams_d", //Changed user name to an existing user
                                   Password = "123qwE*"
                               },
                        AssignedRoleNames = new string[0]
                    }));

            exception.Message.ShouldContain("adams_d");

            //Try to update with existing email address
            exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                await UserAppService.CreateOrUpdateUser(
                    new CreateOrUpdateUserInput
                    {
                        User = new UserEditDto
                               {
                                   Id = jnashUser.Id,
                                   EmailAddress = "adams_d@gmail.com", //Changed email to an existing user
                                   Name = "John",
                                   Surname = "Nash",
                                   UserName = "jnash",
                                   Password = "123qwE*"
                               },
                        AssignedRoleNames = new string[0]
                    }));

            exception.Message.ShouldContain("adams_d@gmail.com");
        }

        [MultiTenantFact]
        public async Task Should_Remove_From_Role()
        {
            LoginAsHostAdmin();

            //Arrange
            var adminUser = await GetUserByUserNameOrNullAsync(AbpUserBase.AdminUserName);
            await UsingDbContextAsync(async context =>
            {
                var roleCount = await context.UserRoles.CountAsync(ur => ur.UserId == adminUser.Id);
                roleCount.ShouldBeGreaterThan(0); //There should be 1 role at least
            });

            //Act
            await UserAppService.CreateOrUpdateUser(
                new CreateOrUpdateUserInput
                {
                    User = new UserEditDto //Not changing user properties
                    {
                        Id = adminUser.Id,
                        EmailAddress = adminUser.EmailAddress,
                        Name = adminUser.Name,
                        Surname = adminUser.Surname,
                        UserName = adminUser.UserName,
                        Password = null
                    },
                    AssignedRoleNames = new[]{ StaticRoleNames.Host.Admin } // Just deleting all roles expect admin
                });

            //Assert
            await UsingDbContextAsync(async context =>
            {
                var roleCount = await context.UserRoles.CountAsync(ur => ur.UserId == adminUser.Id);
                roleCount.ShouldBe(1);
            });
        }

        [MultiTenantFact]
        public async Task Should_Not_Remove_From_Admin_Role()
        {
            LoginAsHostAdmin();

            // Arrange
            var adminUser = await GetUserByUserNameOrNullAsync(AbpUserBase.AdminUserName);
            CreateRole("super_admin");
            
            await UsingDbContextAsync(async context =>
            {
                var roleCount = await context.UserRoles.CountAsync(ur => ur.UserId == adminUser.Id);
                roleCount.ShouldBe(1);
            });
            
            //Act
            await UserAppService.CreateOrUpdateUser(
                new CreateOrUpdateUserInput
                {
                    User = new UserEditDto //Not changing user properties
                    {
                        Id = adminUser.Id,
                        EmailAddress = adminUser.EmailAddress,
                        Name = adminUser.Name,
                        Surname = adminUser.Surname,
                        UserName = adminUser.UserName,
                        Password = null
                    },
                    AssignedRoleNames = new[]{ "super_admin" } // remove admin role and assign super_admin role
                });
            
            // Assert
            var hasSuperAdminRole = await UserManager.IsInRoleAsync(adminUser, "super_admin");
            var hasAdminRole = await UserManager.IsInRoleAsync(adminUser, StaticRoleNames.Host.Admin);
            
            hasSuperAdminRole.ShouldBe(true);
            hasAdminRole.ShouldBe(true);
        }
        
        protected Role CreateRole(string roleName)
        {
            return UsingDbContext(context => context.Roles.Add(new Role(AbpSession.TenantId, roleName, roleName)).Entity);
        }
    }
}

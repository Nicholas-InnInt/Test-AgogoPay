﻿using System;
using System.Collections.Generic;
using Abp.EntityFrameworkCore;
using Neptune.NsPay.EntityFrameworkCore;
using Neptune.NsPay.EntityFrameworkCore.Repositories;
using System.Linq;
using Z.EntityFramework.Plus;

namespace Neptune.NsPay.Authorization.Users
{
    public class UserRepository : NsPayRepositoryBase<User, long>, IUserRepository
    {
        public UserRepository(IDbContextProvider<NsPayDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public List<long> GetPasswordExpiredUserIds(DateTime passwordExpireDate)
        {
            var context = GetContext();

            return (
                from user in GetAll()
                let lastRecentPasswordOfUser = context.RecentPasswords
                    .Where(rp => rp.UserId == user.Id && rp.TenantId == user.TenantId)
                    .OrderByDescending(rp => rp.CreationTime).FirstOrDefault()
                where user.IsActive && !user.ShouldChangePasswordOnNextLogin &&
                      (
                          (lastRecentPasswordOfUser != null &&
                           lastRecentPasswordOfUser.CreationTime <= passwordExpireDate) ||
                          (lastRecentPasswordOfUser == null && user.CreationTime <= passwordExpireDate)
                      )
                select user.Id
            ).Distinct().ToList();
        }

        public void UpdateUsersToChangePasswordOnNextLogin(List<long> userIdsToUpdate)
        {
            GetAll()
                .Where(user =>
                    user.IsActive &&
                    !user.ShouldChangePasswordOnNextLogin &&
                    userIdsToUpdate.Contains(user.Id)
                )
                .Update(x => new User { ShouldChangePasswordOnNextLogin = true });
        }
    }
}

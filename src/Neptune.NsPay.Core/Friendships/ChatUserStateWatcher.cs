﻿using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.RealTime;
using Abp.Threading;
using Neptune.NsPay.Chat;
using Neptune.NsPay.Friendships.Cache;

namespace Neptune.NsPay.Friendships
{
    public class ChatUserStateWatcher : ISingletonDependency
    {
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IOnlineClientManager<ChatChannel> _onlineClientManager;

        public ChatUserStateWatcher(
            IChatCommunicator chatCommunicator,
            IUserFriendsCache userFriendsCache,
            IOnlineClientManager<ChatChannel> onlineClientManager)
        {
            _chatCommunicator = chatCommunicator;
            _userFriendsCache = userFriendsCache;
            _onlineClientManager = onlineClientManager;
        }

        public void Initialize()
        {
            _onlineClientManager.UserConnected += OnlineClientManager_UserConnected;
            _onlineClientManager.UserDisconnected += OnlineClientManager_UserDisconnected;
        }

        private void OnlineClientManager_UserConnected(object sender, OnlineUserEventArgs e)
        {
            AsyncHelper.RunSync(()=> NotifyUserConnectionStateChange(e.User, true));
        }

        private void OnlineClientManager_UserDisconnected(object sender, OnlineUserEventArgs e)
        {
            AsyncHelper.RunSync(()=> NotifyUserConnectionStateChange(e.User, false));
        }

        private async Task NotifyUserConnectionStateChange(UserIdentifier user, bool isConnected)
        {
            var cacheItem = _userFriendsCache.GetCacheItem(user);

            foreach (var friend in cacheItem.Friends)
            {
                var friendUserClients = await _onlineClientManager.GetAllByUserIdAsync(new UserIdentifier(friend.FriendTenantId, friend.FriendUserId));
                if (!friendUserClients.Any())
                {
                    continue;
                }

                AsyncHelper.RunSync(() => _chatCommunicator.SendUserConnectionChangeToClients(friendUserClients, user, isConnected));
            }
        }
    }
}
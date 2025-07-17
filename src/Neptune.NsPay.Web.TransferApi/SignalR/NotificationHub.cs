using Abp.Collections.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Neptune.NsPay.Commons;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SignalRClient;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Threading;

namespace Neptune.NsPay.Web.TransferApi.SignalR
{
    public class NotificationHub : Hub<INotificationHubClient>
    {
        public readonly string systemSecretKey = "Hr8GfmA4d4KfPN8B61puZCco904rgdmw";
        public readonly string internalSecretKey = "gUMxXfnQ3qGaR7VXwl7ig35YThsCNiJ5";
        private static readonly TimeSpan _timeout = TimeSpan.FromMinutes(5);  // Inactive connections timeout
        public static ConcurrentDictionary<string, Tuple<string,DateTime>> Connections = new ConcurrentDictionary<string, Tuple<string, DateTime>>();
        public readonly IPushNotificationService _pushNotificationService;
        public readonly IMemoryCache _memoryCache;
        public NotificationHub(IPushNotificationService pushNotificationService, IMemoryCache memoryCache)
        {
            _pushNotificationService = pushNotificationService;
            _memoryCache = memoryCache;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var merchantCode = httpContext?.Request.Headers["MerchantCode"].ToString();
            var secretKey = httpContext?.Request.Headers["SecretKey"].ToString();
            var sourceType = httpContext?.Request.Headers["CallerSource"];

            if(!sourceType.IsNullOrEmpty() && internalSecretKey.Equals(secretKey, StringComparison.OrdinalIgnoreCase))
            {
                _memoryCache.Set(GetCacheKey(Context.ConnectionId), string.Empty, TimeSpan.FromDays(2));
                NlogLogger.Info("SignalRNotificationHub - Internal Connected (" + Context.ConnectionId + ")");
            }
            else if (!merchantCode.IsNullOrEmpty() && !secretKey.IsNullOrEmpty()  && systemSecretKey.Equals(secretKey, StringComparison.OrdinalIgnoreCase))
            {
                var merchantDatetimePair = Tuple.Create(merchantCode, DateTime.Now);
                Connections.AddOrUpdate(this.Context.ConnectionId, merchantDatetimePair, (key, oldValue) => merchantDatetimePair);
                NlogLogger.Info("SignalRNotificationHub - Merchant Connected (" + Context.ConnectionId + ") +(" + merchantCode + ")");
                await JoinGroup(merchantCode);
                SendFirstPushAfterDelayAsync(2000, merchantCode);
            }
            else
            {
                await Clients.Caller.SendMessage("Error", "Unauthorized: Missing or invalid token.");
                Context.Abort();  // This will forcefully close the connection
                return;
            }

            Console.WriteLine($"User ID: {merchantCode}, Token: {secretKey}");
            await base.OnConnectedAsync();
        }


        private string GetCacheKey(string connectionId)
        {
            return string.Format("{0}:{1}", "InternalAccess", connectionId);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Connections.TryRemove(Context.ConnectionId, out var merchantCode);
            NlogLogger.Info("SignalRNotificationHub - Merchant Disconnected (" + Context.ConnectionId + ") +(" + merchantCode ?? string.Empty + ")");
            _memoryCache.Remove(GetCacheKey(Context.ConnectionId));
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Connection {Context.ConnectionId} is joining group {groupName}");
        }

        public static List<string> OnlineMerchant()
        {
            return Connections.Values.Select(x=>x.Item1).ToList();
        }
        private async void SendFirstPushAfterDelayAsync(int delayInMilliseconds, string merchantCode)
        {
            // Wait asynchronously for the specified delay
            await Task.Delay(delayInMilliseconds);
            await _pushNotificationService.NotifyMerchant(merchantCode);
        }


        // Write a Wroker To Clean Up By Interval
        public static void CleanupInactiveConnections()
        {
            try
            {
            var now = DateTime.UtcNow;
            var inactiveConnections = Connections
            .Where(kvp => now - kvp.Value.Item2 > _timeout)  // Check if the connection is inactive
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var connectionId in inactiveConnections)
            {
                // Remove inactive connections
                Connections.TryRemove(connectionId, out _);
                Console.WriteLine($"Removed inactive connection: {connectionId}");
            }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("SignalRNotificationHub Error ", ex);
            }
          
        }

        public async Task WithdrawalDeviceChangedInternal( WithdrawalDeviceChangedDto withdrawalDeviceInfo)
        {
            if (_memoryCache.TryGetValue(GetCacheKey(Context.ConnectionId), out string cachedData))
            {
                NlogLogger.Info("SignalRNotificationHub -  Withdrawal Device Changed Internal (" + Context.ConnectionId + ") Data - (" + JsonConvert.SerializeObject(withdrawalDeviceInfo) + ")");
                await _pushNotificationService.IdentifyAndNotifyDeviceChanged(withdrawalDeviceInfo);
            }
        }
    }

    public interface INotificationHubClient
    {
        Task SendMessage(string user, string message);
        Task UpdateBalanceTransfer(BalanceChangedNotification notificationContent);
        Task WithdrawDeviceChanged(MerchantDeviceChangedNotification notificationContent);
    }

    public class BalanceChangedNotification
    {
        public long VersionNumber { get; set; }
        public int DeviceId { get; set; }
        public decimal Balance { get; set; }

    }

    public class MerchantDeviceChangedNotification
    {
        public long VersionNumber { get; set; }
        public string  MerchantCode { get; set; }
        public List<WithdrawalDeviceRedisModel> WithdrawalDevices { get; set; }
        public MerchantDeviceChangedNotification()
        {
            WithdrawalDevices = new List<WithdrawalDeviceRedisModel>();
        }
    }
}

using Abp.Collections.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Neptune.NsPay.Commons;
using Neptune.NsPay.SignalRClient;
using Neptune.NsPay.Web.PayMonitorApi.Models;
using Neptune.NsPay.Web.PayMonitorApi.Models.SignalR;
using System.Collections.Concurrent;

namespace Neptune.NsPay.Web.PayMonitorApi.Helpers
{
    public class NotificationHub : Hub<INotificationHubClient>
    {

        public static ConcurrentDictionary<string, string> Connections = new ConcurrentDictionary<string, string>();

        public readonly string systemSecretKey = "Hr8GfmA4d4KfPN8B61puZCco904rgdmw";
        public readonly string internalSecretKey = "gUMxXfnQ3qGaR7VXwl7ig35YThsCNiJ5";
        public readonly IPayMonitorCommonHelpers _payMonitorCommonHelpers;
        public readonly IMemoryCache _memoryCache;

        public NotificationHub(IPayMonitorCommonHelpers payMonitorCommonHelpers, IMemoryCache memoryCache)
        {
            _payMonitorCommonHelpers = payMonitorCommonHelpers;
            _memoryCache = memoryCache;
        }

        private string GetCacheKey(string connectionId)
        {
            return string.Format("{0}:{1}", "InternalAccess", connectionId);
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var merchantCode = httpContext?.Request.Headers["MerchantCode"].ToString();
            var secretKey = httpContext?.Request.Headers["SecretKey"].ToString();
            var sourceType = httpContext?.Request.Headers["CallerSource"];

            if(!sourceType.IsNullOrEmpty()&& internalSecretKey.Equals(secretKey, StringComparison.OrdinalIgnoreCase))
            {
                // Internal Connection
                _memoryCache.Set(GetCacheKey(Context.ConnectionId), string.Empty, TimeSpan.FromDays(2));
                NlogLogger.Info("SignalRHub - Internal Connected (" + Context.ConnectionId + ")");
            }
            else if (!merchantCode.IsNullOrEmpty() && systemSecretKey.Equals(secretKey, StringComparison.OrdinalIgnoreCase))
            {
                Connections.AddOrUpdate(this.Context.ConnectionId, merchantCode, (key, oldValue) => merchantCode);
                await JoinGroup(merchantCode);
                SendFirstPushAfterDelayAsync(2000 , merchantCode);
                NlogLogger.Info("SignalRHub - Merchant Connected (" + Context.ConnectionId + ") +("+ merchantCode + ")");
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

        private async void SendFirstPushAfterDelayAsync(int delayInMilliseconds , string merchantCode)
        {
            // Wait asynchronously for the specified delay
            await Task.Delay(delayInMilliseconds);
            await _payMonitorCommonHelpers.InitialDataMerchant(merchantCode);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Connections.TryRemove(Context.ConnectionId, out var merchantCode);
            Console.WriteLine($"Disconnected: {Context.ConnectionId} - MerchantCode: {merchantCode}");
            _memoryCache.Remove(GetCacheKey(Context.ConnectionId));
            NlogLogger.Info("SignalRHub - Merchant Disconnected (" + Context.ConnectionId + ") +(" + merchantCode ?? string.Empty + ")");
            await base.OnDisconnectedAsync(exception);
        }

        public static List<string> GetOnlineMerchantCode()
        {
            return Connections.Select(x => x.Value).Distinct().ToList();
        }

        //groupname = merchantCOde
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Connection {Context.ConnectionId} is joining group {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task UpdateBalance(BalanceUpdateNotification input)
        {
            await Clients.All.UpdateBalance(input);
        }


        public async Task MerchantPaymentChanged(MerchantPaymentDetails input)
        {
            NlogLogger.Info("SignalRHub - MerchantPaymentChanged (" + input.MerchantCode + ")");
            await Clients.Group(input.MerchantCode).MerchantPaymentChanged(input);
        }
        public async Task MerchantPaymentChangedInternal(MerchantPaymentChangedDto merchantPaymentInfo)
        {
            if(_memoryCache.TryGetValue(GetCacheKey(Context.ConnectionId), out string cachedData))
            {
                NlogLogger.Info("SignalRHub - Payment Changed Internal (" + Context.ConnectionId + ") Data - ("+ JsonConvert.SerializeObject( merchantPaymentInfo) + ")");
                await _payMonitorCommonHelpers.DetermineChangesAndNotify(merchantPaymentInfo);
            }
        }
      
    }
}

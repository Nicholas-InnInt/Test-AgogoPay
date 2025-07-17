using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.SignalR;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Neptune.NsPay.Web.SignalR.MerchantBalance
{
    public class MerchantSignalRContent
    {
        public string MerchantCode { get; set; }
        public long VersionNumber { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal LockedBalance { get; set; }
        public decimal USDTCurrentBalance { get; set; }
        public decimal USDTLockedBalance { get; set; }
    }

    public class MerchantHub : Hub<IMerchantHubClient>
    {
        private static readonly ConcurrentDictionary<string, DateTime> _connections = new ConcurrentDictionary<string, DateTime>();
        private static readonly ConcurrentDictionary<string, Tuple<string, DateTime>> _connectionsMerchant = new ConcurrentDictionary<string, Tuple<string, DateTime>>();

        private static readonly TimeSpan _timeout = TimeSpan.FromMinutes(5);  // Inactive connections timeout
        private static readonly TimeSpan _timeoutOption = TimeSpan.FromMinutes(30);  // Inactive connections selection timeout

        private readonly IAbpSession _abpSession;
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private Task _merchantTask;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public MerchantHub(IAbpSession abpSession, UserManager userManager, IUnitOfWorkManager unitOfWorkManager, IMerchantFundsMongoService merchantFundsMongoService)
        {
            _abpSession = abpSession;
            _userManager = userManager;
            _unitOfWorkManager = unitOfWorkManager;
            _merchantFundsMongoService = merchantFundsMongoService;
        }

        //public async Task SendMessage(string message)
        //{
        //    // Broadcast message to all connected clients
        //    await Clients.All.ReceiveMessage(message);
        //}

        public static Dictionary<string, Tuple<string, DateTime>> GetAllOnlineUser()
        {
            return _connectionsMerchant.ToDictionary();
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                string connectionId = Context.ConnectionId;
                Console.WriteLine($"Client connected. Connection ID: {connectionId}");

                // Perform any logic, such as notifying other clients or saving connection info

                var currentUser = await GetCurrentUserAsync();

                if (currentUser != null)
                {
                    // Capture the connection ID and log or perform actions
                    var cDate = DateTime.Now;
                    _connections[connectionId] = cDate;
                    _connectionsMerchant.TryAdd(connectionId, Tuple.Create(currentUser.Merchants.First().MerchantCode, cDate));
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("MerchantHub - error ", ex);
            }

            await base.OnConnectedAsync();
        }

        // Heartbeat method to refresh the connection's timestamp
        public Task Heartbeat()
        {
            string connectionId = Context.ConnectionId;
            _connections[connectionId] = DateTime.Now;
            Console.WriteLine($"Heartbeat received from: {connectionId}");
            return Task.CompletedTask;
        }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = default(User);
            using (var uow = _unitOfWorkManager.Begin())
            {
                try
                {
                    user = await _userManager.FindByIdAsync(_abpSession.GetUserId().ToString());
                    if (user == null)
                    {
                        throw new Exception("There is no current user!");
                    }
                    //获取商户信息
                    user.Merchants = await _userManager.GetUserMerchantAsync(user);
                }
                catch (Exception ex)
                {
                }

                await uow.CompleteAsync();
            }

            return user;
        }

        private async Task<MerchantSignalRContent> GetSignalContent(string merchantCode)
        {
            var merchantInfo = await _merchantFundsMongoService.GetFundsByMerchantCode(merchantCode);
            var merchantSignalrDetails = new MerchantSignalRContent() { MerchantCode = merchantCode, VersionNumber = TimeHelper.GetUnixTimeStamp(DateTime.Now) };

            if (merchantInfo != null)
            {
                merchantSignalrDetails.CurrentBalance = merchantInfo.Balance;
                merchantSignalrDetails.LockedBalance = merchantInfo.FrozenBalance;
                merchantSignalrDetails.USDTCurrentBalance = merchantInfo.USDTBalance;
                merchantSignalrDetails.USDTLockedBalance = merchantInfo.USDTFrozenBalance;
            }

            return merchantSignalrDetails;
        }

        public async Task GetMerchantInfo()
        {
            // Broadcast message to all connected clients
            var currentUser = await GetCurrentUserAsync();
            if (currentUser != null)
            {
                //filterObj.MerchantIds = currentUser.Merchants.Select(x => x.Id).ToList();
                if (_connectionsMerchant.TryGetValue(Context.ConnectionId, out Tuple<string, DateTime> connection))
                {
                    await Clients.Client(Context.ConnectionId).MerchantInfoUpdate(await GetSignalContent(connection.Item1));
                }
            }
        }

        public static Dictionary<string, DateTime> GetOnlineUser()
        {
            return _connections.ToDictionary();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;
            if (_connections.TryRemove(connectionId, out var _))
            {
                _connectionsMerchant.Remove(connectionId);
            }

            Console.WriteLine($"Client disconnected. Connection ID: {connectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public static void CleanupInactiveConnections()
        {
            var now = DateTime.Now;
            var inactiveConnections = _connections
                .Where(kvp => now - kvp.Value > _timeout)  // Check if the connection is inactive
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var connectionId in inactiveConnections)
            {
                // Remove inactive connections
                _connections.TryRemove(connectionId, out _);
                Console.WriteLine($"Removed inactive connection: {connectionId}");
            }

            var inactiveOptionConnections = _connectionsMerchant
                .Where(kvp => now - kvp.Value.Item2 > _timeoutOption)  // Check if the connection is inactive
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var connectionId in inactiveOptionConnections)
            {
                // Remove inactive connections
                _connectionsMerchant.TryRemove(connectionId, out _);
                Console.WriteLine($"Removed inactive connection merchant: {connectionId}");
            }
        }
    }
}
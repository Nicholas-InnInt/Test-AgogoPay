using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.SignalR;
using Neptune.NsPay.PayOrders.Dtos;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Neptune.NsPay.Authorization.Users;
using System.Linq;
using Newtonsoft.Json;

namespace Neptune.NsPay.Web.SignalR
{
    public class OrderTrackerHub : Hub<IOrderTrackerHubClient>
    {
        private static readonly ConcurrentDictionary<string, DateTime> _connections = new ConcurrentDictionary<string, DateTime>();
        private static readonly ConcurrentDictionary<string, Tuple<GetAllPayOrdersInput, DateTime>> _connectionsSelectOption = new ConcurrentDictionary<string, Tuple<GetAllPayOrdersInput, DateTime>>();

        private static readonly TimeSpan _timeout = TimeSpan.FromMinutes(5);  // Inactive connections timeout
        private static readonly TimeSpan _timeoutOption = TimeSpan.FromMinutes(30);  // Inactive connections selection timeout

        private readonly IAbpSession _abpSession;
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public OrderTrackerHub(IAbpSession abpSession, UserManager userManager, IUnitOfWorkManager unitOfWorkManager)
        {
            _abpSession = abpSession;
            _userManager = userManager;
            _unitOfWorkManager = unitOfWorkManager;
        }
        public async Task SendMessage(string message)
        {
            // Broadcast message to all connected clients
            await Clients.All.ReceiveMessage(message);
        }

        public override async Task OnConnectedAsync()
        {
            // Capture the connection ID and log or perform actions
            string connectionId = Context.ConnectionId;
            Console.WriteLine($"Client connected. Connection ID: {connectionId}");
            _connections[connectionId] = DateTime.Now;
            // Perform any logic, such as notifying other clients or saving connection info

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

        public async Task SetOrderFilter(GetAllPayOrdersInput filterObj)
        {
            // Broadcast message to all connected clients
            var currentUser = await GetCurrentUserAsync();
            if (currentUser != null)
            {
                filterObj.MerchantIds = currentUser.Merchants.Select(x => x.Id).ToList();
            }

            _connectionsSelectOption.AddOrUpdate(Context.ConnectionId, Tuple.Create(filterObj, DateTime.Now), (key, oldValue) => Tuple.Create(filterObj, DateTime.Now));
        }

        public static Dictionary<string, DateTime> GetOnlineUser()
        {
            return _connections.ToDictionary();
        }

        public static Dictionary<string, GetAllPayOrdersInput> GetOnlineUserSelectOption()
        {
            return _connections.ToDictionary(x => x.Key, x => _connectionsSelectOption.TryGetValue(x.Key, out Tuple<GetAllPayOrdersInput, DateTime> _out) ? DeepClone< GetAllPayOrdersInput >( _out.Item1) : null);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;
            _connections.TryRemove(connectionId, out _);
            Console.WriteLine($"Client disconnected. Connection ID: {connectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        private static T DeepClone<T>(T initialObj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(initialObj));
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


            var inactiveOptionConnections = _connectionsSelectOption
                .Where(kvp => now - kvp.Value.Item2 > _timeoutOption)  // Check if the connection is inactive
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var connectionId in inactiveOptionConnections)
            {
                // Remove inactive connections
                _connectionsSelectOption.TryRemove(connectionId, out _);
                Console.WriteLine($"Removed inactive connection selectionoption: {connectionId}");
            }

        }

    }

    public class signalRGetPayOrderInputDto
    {
        public string Filter { get; set; }

        public string MerchantCodeFilter { get; set; }
        public string OrderNoFilter { get; set; }
        public string OrderMarkFilter { get; set; }

        public int? OrderPayTypeFilter { get; set; }
        public int? OrderTypeFilter { get; set; }

        public int? OrderStatusFilter { get; set; }

        public int? ScoreStatusFilter { get; set; }

        public DateTime? OrderCreationTimeStartDate { get; set; }

        public DateTime? OrderCreationTimeEndDate { get; set; }

        public string UtcTimeFilter { get; set; } = "GMT7+";

        public string PayMentPhoneFilter { get; set; }

        public decimal? MinOrderMoneyFilter { get; set; }
        public decimal? MaxOrderMoneyFilter { get; set; }

    }
}
using Abp.Collections.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Commons;
using Neptune.NsPay.SignalRClient;
using Neptune.NsPay.Web.TransferApi.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;

namespace Neptune.NsPay.Web.TransferApi.SignalR
{
    public class OrderHub : Hub<IOrderHubClient>
    {
        public static ConcurrentDictionary<int, Tuple<string,HubCallerContext>> Connections = new ConcurrentDictionary<int, Tuple<string, HubCallerContext>>();
       
        public static ConcurrentDictionary<string,DateTime> ConnectionDate = new ConcurrentDictionary<string, DateTime>();
        private static readonly TimeSpan _connectionTimeOut = TimeSpan.FromMinutes(500);  // Froce Disconnect 
        public static ConcurrentDictionary<string, Tuple<HubCallerContext,DateTime>> ConnectionAbortList = new ConcurrentDictionary<string, Tuple<HubCallerContext, DateTime>>();
        private readonly PushOrderHelper _pushOrderHelper;
        public OrderHub(PushOrderHelper pushOrderHelper)
        {
            _pushOrderHelper = pushOrderHelper;
        }

        public static string GetDeviceConnection(int deviceId)
        {
            if(Connections.TryGetValue(deviceId , out Tuple<string, HubCallerContext> connectionId))
            {
                return connectionId.Item1;
            }
            else
            {
                return null;
            }
        }

        private string GetCacheKey(string connectionId)
        {
            return string.Format("{0}:{1}", "InternalAccess", connectionId);
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var deviceId = httpContext?.Request.Headers["DeviceId"].ToString();
            var transport = httpContext?.Request.Headers["Sec-WebSocket-Protocol"];
            var transportWith = httpContext?.Request.Headers["X-Requested-With"];
            var clientIp = Context.GetHttpContext()?.Connection?.RemoteIpAddress;


            var headers = httpContext?.Request.Headers;

            var headerStr = string.Empty;
            // Log each header
            foreach (var header in headers)
            {
                headerStr += ("Header: {Key} = {Value}", header.Key, header.Value)+Environment.NewLine;
            }

            if (!deviceId.IsNullOrEmpty() && int.TryParse(deviceId, out int deviceIdInt))
            {
                var newValue = Tuple.Create(Context.ConnectionId, Context);
                Connections.AddOrUpdate(deviceIdInt, newValue, (key, oldValue) => {

                    try
                    {
                        //ConnectionAbortList.TryAdd(oldValue.Item1, Tuple.Create(oldValue.Item2, DateTime.Now.AddMinutes(3)));
                        // oldValue.Item2.Abort();
                        // send to channel DisconnectClient instead of direct Abort Connection
                        //Clients.Client(oldValue.Item2.ConnectionId).DisconnectClient(new SignalRConnection { DeviceId = deviceIdInt, ConnectionId = oldValue.Item2.ConnectionId });

                    }
                    catch(Exception ex)
                    {
                        NlogLogger.Error("SignalRHub Error - Device Connection Removed (" + oldValue.Item2.ConnectionId + ")");
                    }
                  
                    NlogLogger.Info("SignalRHub - Device Connection Removed (" + oldValue.Item2.ConnectionId + ")");

                    return newValue;
                });

                ConnectionDate.TryAdd(Context.ConnectionId, DateTime.Now);
                NlogLogger.Info("SignalRHub - Device Connected (" + Context.ConnectionId + ") + Protocol (" + transport + ") Request Header ("+ headerStr + ") Client IP ("+ clientIp + ")");
                Console.WriteLine("SignalRHub - Device Connected (" + Context.ConnectionId + ") + Protocol (" + transport + ") Request Protocol (" + transportWith + ") Device ID ("+ deviceId + ")");
            }
            else
            {
                Context.Abort();  // This will forcefully close the connection
                return;
            }

            await base.OnConnectedAsync();

        }

        public static async Task HouseKeepConnection(List<int> NotFreeDevices)
        {
            var cDate = DateTime.Now;
            NlogLogger.Info("HouseKeepConnection Start");
            try
            {
                foreach (var device in ConnectionDate.Where(x => x.Value < cDate.Add(-_connectionTimeOut)))
                {
                    var existingDeviceConnection = Connections.FirstOrDefault(x => x.Value.Item1 == device.Key);
                    if (existingDeviceConnection.Key > 0)
                    {

                        if (NotFreeDevices.Contains(existingDeviceConnection.Key))
                        {
                            continue;
                        }
                        else
                        {
                            var connectedMilliSecond = (cDate - device.Value).TotalMilliseconds;
                            //NlogLogger.Info("SignalRHub - Force Disconnected Due To Timeout " + connectedMilliSecond + " ms (" + existingDeviceConnection.Value.Item1 + ")(" + existingDeviceConnection.Key + ")");
                            //existingDeviceConnection.Value.Item2.Abort();
                            ConnectionDate.TryRemove(device);
                        }
                    }
                    else
                    {
                        ConnectionDate.TryRemove(device);
                    }

                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("HouseKeepConnection - Error ", ex);
            }

            NlogLogger.Info("HouseKeepConnection End Existing Connection Count (" + ConnectionDate.Count() + ")");
            
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {

            string deviceId = string.Empty;
            foreach (var kvp in Connections)
            {
                if (kvp.Value.Item1 == Context.ConnectionId)
                {
                    // Try to remove the entry using the key
                    Connections.TryRemove(kvp.Key, out _);
                    deviceId = kvp.Key.ToString();
                }
            }

            ConnectionDate.TryRemove(Context.ConnectionId, out DateTime _connectDate);

            if(!string.IsNullOrEmpty(deviceId)&& Int32.TryParse(deviceId , out var _value))
            {
               await _pushOrderHelper.SaveDisconnectedDeviceOrder(_value);
            }
            Console.WriteLine($"Disconnected: {Context.ConnectionId} - DeviceId: {deviceId}");
            NlogLogger.Info("SignalRHub - Device Disconnected (" + Context.ConnectionId + ") - "+ _connectDate + " (" + (deviceId ?? string.Empty) + ")");
            await base.OnDisconnectedAsync(exception);
        }

        public static List<int> GetConnectedDevice()
        {
            return Connections.Keys.ToList();
        }

        public static List<Tuple<int,string>> GetConnectedDeviceConnection()
        {
            return Connections.Select(x=> Tuple.Create(x.Key , x.Value.Item1)).ToList();
        }

        public  async Task  DeviceStatusUpdate(DeviceStatusUpdateContent content)
        {
            if (Connections.TryGetValue(content.DeviceId, out Tuple<string,HubCallerContext> connection))
            {
                _pushOrderHelper.DeviceStatusUpdate(content.DeviceId, content.CurrentStatus);
            }
        }

        public async Task RequestDeviceOrder(RequestDeviceOrderContent content)
        {
            if (Connections.TryGetValue(content.DeviceId, out Tuple<string, HubCallerContext> connection))
            {
                _pushOrderHelper.SendDeviceCurrentOrder(content.DeviceId);
            }
        }


        public async Task ProcessOrderClient(SignalROrderInfo content)
        {
            if (Connections.TryGetValue(content.DeviceId, out Tuple<string, HubCallerContext> connection))
            {
                await Clients.Client(connection.Item1).ProcessOrder(content);
            }
        }


        public async Task SendMessage(string user , string message)
        {
            await Clients.All.SendMessage(user, message);
        }

        public async Task SendMessageObject(userObj user, string message)
        {
            await Clients.All.SendMessage(JsonConvert.SerializeObject(user), message);
        }

        public class userObj
        {
            public string UserId { get; set; }

            public string Password { get; set; }
        }
        public class DeviceStatusUpdateContent
        {
            public int DeviceId { get; set; }
            public string? OrderId { get; set; }
            public string Remark { get; set; }
            public DeviceProcessStatusEnum CurrentStatus { get; set; }

        }

        public class RequestDeviceOrderContent
        {
            public int DeviceId { get; set; }

        }

        }
    }

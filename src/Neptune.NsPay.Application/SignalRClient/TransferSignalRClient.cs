using Microsoft.AspNetCore.SignalR.Client;
using Neptune.NsPay.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SignalRClient
{
    public class TransferSignalRClient : ITransferSignalRClient
    {
        private readonly IAppConfigurationAccessor _configurationAccessor;
        private readonly HubConnection _hubConnection;
        private const string withdrawalDeviceChanged = "WithdrawalDeviceChangedInternal";

        public TransferSignalRClient(IAppConfigurationAccessor configurationAccessor)
        {
            _configurationAccessor = configurationAccessor;
            var signalRServerUrl = _configurationAccessor.Configuration["TransferSignalR:ServerUrl"];
            var signalRToken = _configurationAccessor.Configuration["TransferSignalR:SecretToken"];

            if (!string.IsNullOrEmpty(signalRServerUrl))
            {
                _hubConnection = new HubConnectionBuilder()
           .WithUrl(signalRServerUrl, options =>
           {
               options.Headers.Add("SecretKey", signalRToken);
               options.Headers.Add("CallerSource", "Internal");
           }).WithAutomaticReconnect(new AlwaysRetryPolicy())  // Adjust the URL to your SignalR server's endpoint
           .Build();

                _hubConnection.ServerTimeout = TimeSpan.FromMinutes(3);
                _hubConnection.KeepAliveInterval = TimeSpan.FromSeconds(30);


                _hubConnection.Reconnected += async (connectionId) =>
                {
                    Console.WriteLine("Reconnected to the SignalR hub with connection ID: " + connectionId);

                };

                _hubConnection.Closed += async (error) =>
                {
                    Console.WriteLine("Connection closed. Reconnecting...");
                };
            }
        }


        private sealed class AlwaysRetryPolicy : IRetryPolicy
        {
            public TimeSpan? NextRetryDelay(RetryContext retryContext)
            {
                return TimeSpan.FromMilliseconds(RandomDelayMilliseconds());
            }
        }

        private static int RandomDelayMilliseconds()
        {
            // Delay will be between 5 to 10 secs
            return new Random().Next(5000, 10000);
        }

        private async Task<bool> StartConnection()
        {
            var isSuccess = true;
            try
            {
                // Possible to null because of  appsetting content
                if (_hubConnection == null)
                {
                    isSuccess = false;

                }
                else if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                }

            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        public async Task<bool> WithdrawalDeviceChanged(WithdrawalDeviceChangedDto deviceChanged)
        {
            if (await StartConnection())
            {
                await _hubConnection.InvokeAsync(withdrawalDeviceChanged, deviceChanged);
            }

            return true;
        }
    }
}


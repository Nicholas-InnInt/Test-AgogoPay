using Microsoft.Extensions.Logging;
using Neptune.NsPay.Commons;
using System.Threading;
using System;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Neptune.NsPay.Web.SignalR.MerchantBalance;

namespace Neptune.NsPay.Web.SignalR
{
    public class SignalRTaskScheduler : BackgroundService
    {
        private readonly ILogger<SignalRTaskScheduler> _logger;
        private readonly int schedulerTaskMiliSeconds = 10000;
        private readonly MerchantHubService _merchantHubService;

        public SignalRTaskScheduler(ILogger<SignalRTaskScheduler> logger , MerchantHubService merchantHubService)
        {
            _logger = logger;
            _merchantHubService = merchantHubService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(schedulerTaskMiliSeconds, stoppingToken);  //  milliseconds delay
            int currentCounter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _merchantHubService.TriggerOnlineMerchant();
                }
                catch (Exception ex)
                {
                    NlogLogger.Error(ex.Message);
                }

                currentCounter++;
                await Task.Delay(schedulerTaskMiliSeconds, stoppingToken);  // milliseconds delay
            }
        }

    }
}
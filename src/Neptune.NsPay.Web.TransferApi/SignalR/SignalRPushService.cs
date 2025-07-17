using Neptune.NsPay.Commons;

namespace Neptune.NsPay.Web.TransferApi.SignalR
{
    public class SignalRPushService : BackgroundService
    {
        private readonly ILogger<SignalRPushService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly int schedulerTaskMiliSeconds = 15000;

        public SignalRPushService(ILogger<SignalRPushService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(schedulerTaskMiliSeconds, stoppingToken);  //  milliseconds delay
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        // Resolve your scoped services
                        var scopedService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();

                        // Use the resolved service (e.g., make a call)
                        await scopedService.NotifyAllMerchant();

                        NotificationHub.CleanupInactiveConnections(); // Clear Unuse connection
                    }
                }
                catch (Exception ex)
                {
                    NlogLogger.Error(ex.Message);
                }

                await Task.Delay(schedulerTaskMiliSeconds, stoppingToken);  // milliseconds delay
            }
        }

    }
}
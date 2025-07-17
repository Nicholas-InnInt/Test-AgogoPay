using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Neptune.NsPay.Commons;
using Neptune.NsPay.KafkaExtensions;

namespace Neptune.NsPay.Web.KafkaApi.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private ILogger<KafkaConsumerService> _logger;
        private readonly KafkaOptions _options;

        //private readonly KafkaHandlerDispatcher _dispatcher;
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerService(IOptions<KafkaOptions> options,
             //KafkaHandlerDispatcher dispatcher,
             IServiceProvider serviceProvider, ILogger<KafkaConsumerService> logger)
        {
            _logger = logger;
            _options = options.Value;
            //_dispatcher = dispatcher;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.GroupId,
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnablePartitionEof = true,
                //SaslUsername = _options.SaslUsername,
                //SaslPassword = _options.SaslPassword,
                //SecurityProtocol = SecurityProtocol.SaslSsl,
                //SaslMechanism = SaslMechanism.Plain
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_options.Topics);

            //CancellationTokenSource cts = new CancellationTokenSource();
            //Console.CancelKeyPress += (_, e) => {
            //    e.Cancel = true;
            //    cts.Cancel();
            //};

            try
            {
                while (true)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    try
                    {
                        var result = consumer.Consume(stoppingToken); // 直接用 framework 的 token
                        if (result != null && result.Message != null)
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var dispatcher = scope.ServiceProvider.GetRequiredService<KafkaHandlerDispatcher>();
                                await dispatcher.DispatchAsync(result.Topic, result.Message.Key, result.Message.Value);
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        NlogLogger.Error($"Kafka消费分发异常: {ex.Message}", ex);
                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error($"Kafka消费Handler异常: {ex.Message}", ex);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                NlogLogger.Error($"Kafka消费异常: {ex.Message}", ex);
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
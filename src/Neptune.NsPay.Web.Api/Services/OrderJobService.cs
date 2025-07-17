using Neptune.NsPay.BillingExtensions;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.AccountCheckerClientExtension;
using System.Text.Json;
using Neptune.NsPay.VietQR;
using Org.BouncyCastle.Asn1.X9;
using System.Text.Json.Serialization;

namespace Neptune.NsPay.Web.Api.Services
{
    public class OrderJobService 
    {
        private readonly BlockingCollection<QueryOrderRecipientQueueItems> _queue = new BlockingCollection<QueryOrderRecipientQueueItems>(boundedCapacity: 100);
        private Task _consumerTask;
        private bool _isStarted;
        private readonly AccountCheckerService _accountCheckerService;

        public OrderJobService(AccountCheckerService accountCheckerService)
        {
            _accountCheckerService = accountCheckerService;
        }

        public void OnStart(CancellationToken cancellationToken)
        {
            if (_isStarted) return;

            _consumerTask = Task.Factory.StartNew(async () => await ProcessQueueItem(cancellationToken), TaskCreationOptions.LongRunning);
            _isStarted = true;
        }

        public void OnStop(CancellationToken cancellationToken)
        {
            if (!_isStarted) return;

            _queue.CompleteAdding();
            _consumerTask.Wait();
            _isStarted = false;
        }
        private async Task ProcessQueueItem(CancellationToken cancellationToken)
        {
            // Proccess only thos update which using mongodb transaction 

            foreach (var item in _queue.GetConsumingEnumerable(cancellationToken))
            {
                var sw = Stopwatch.StartNew();
                var cDate = DateTime.Now;
                var isNameVerfied = false;

                try
                {
                    var bankKeyStr = WithdrawalOrderBankMapper.FindBankByName(item.RecipientBankName);

                    if(!string.IsNullOrEmpty(bankKeyStr))
                    {
                        var result  = await _accountCheckerService.CheckAccountName(bankKeyStr, item.RecipientAccountNumber ,item.RecipientAccountName , item.MerchantId);

                        if(result.HasValue)
                        {
                            isNameVerfied = result.Value;
                        }
                    }
                    item.Callback?.Invoke(isNameVerfied);

                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Query Recipient Job  Cancelled");
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("Query Recipient Job Failed", ex);
                }
                finally
                {
                    sw.Stop();
                    NlogLogger.Warn("Query Recipient Job Processor [" + JsonSerializer.Serialize(item) + "] Time Taken " + sw.ElapsedMilliseconds + " ms Delay   " + (cDate - item.SubmitDateTime).TotalMilliseconds + "  ms Queue Count (" + _queue.Count() + ") Have Info Added ("+ isNameVerfied + ")");
                }

            }
        }

        public bool AddQueryRecipientQueue(QueryOrderRecipientQueueItems queueItem)
        {
            return _queue.TryAdd(queueItem);
        }
    }

    public class OrderJobServiceHost : IHostedService
    {

        private readonly OrderJobService _orderJobService;
        public OrderJobServiceHost(OrderJobService orderJobService)
        {
            _orderJobService = orderJobService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _orderJobService.OnStart(cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _orderJobService.OnStop(cancellationToken);
            return Task.CompletedTask;
        }


    }

    public class QueryOrderRecipientQueueItems
    {
        public string RecipientBankName { get; set; }
        public string RecipientAccountNumber { get; set; }
        public string RecipientAccountName { get; set; }

        public int MerchantId { get; set; }


        public DateTime SubmitDateTime { get; set; }

        [JsonIgnore]
        public Action<bool>? Callback { get; set; }
    }
}

using Microsoft.AspNetCore.SignalR;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Neptune.NsPay.Web.SignalR.MerchantBalance
{
    public class MerchantHubService
    {
        private readonly IHubContext<MerchantHub, IMerchantHubClient> _hub;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;

        public MerchantHubService(IHubContext<MerchantHub, IMerchantHubClient> hub, IMerchantFundsMongoService merchantFundsMongoService)
        {
            _hub = hub;
            _merchantFundsMongoService = merchantFundsMongoService;
        }

        public async Task TriggerOnlineMerchant()
        {
            var eventId = Guid.NewGuid().ToString();
            var affectedClientList = new ConcurrentBag<string>();
            var _connectionsMerchant = MerchantHub.GetAllOnlineUser();
            try
            {
                var semaphore = new SemaphoreSlim(3); // Limit concurrent inserts

                List<Task> merchantUpdateTask = new List<Task>();

                foreach (var merchant in _connectionsMerchant.GroupBy(x => x.Value.Item1).Distinct())
                {
                    merchantUpdateTask.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await semaphore.WaitAsync(); // Prevent simultaneous inserts
                            var merchantInfo = await GetSignalContent(merchant.Key);

                            foreach (var clientId in merchant.Select(x => x.Key))
                            {
                                affectedClientList.Add(clientId + "_" + merchant.Key);
                                await _hub.Clients.Clients(clientId).MerchantInfoUpdate(merchantInfo);
                            }
                        }
                        finally
                        {
                            semaphore.Release(); // Release after execution
                        }
                    }));
                }
                await Task.WhenAll(merchantUpdateTask); // Wait for all insert tasks to complete
            }
            catch (Exception ex)
            {
                NlogLogger.Error("[" + eventId + "]sendMerchantUpdate Error", ex);
            }
            finally
            {
                NlogLogger.Warn("[" + eventId + "] sendMerchantUpdate Updated Node " + string.Join(",", affectedClientList));
            }
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
    }
}
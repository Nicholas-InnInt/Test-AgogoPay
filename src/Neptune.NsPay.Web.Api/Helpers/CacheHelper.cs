using Neptune.NsPay.Commons;
using System.Collections.Concurrent;

namespace Neptune.NsPay.Web.Api.Helpers
{
    public class MerchantOrderCacheHelper
    {
        private ConcurrentDictionary<Tuple<int, string, string>, Tuple<DateTime, decimal>> merchantOrderCache = new ConcurrentDictionary<Tuple<int, string, string>, Tuple<DateTime, decimal>>();
        private ConcurrentDictionary<string, DateTime> AutoOrderCache = new ConcurrentDictionary<string, DateTime>();
        private int houseKeepCount = 0;

        #region Merchant Cache

        public string AddMerchantOrder(int MerchantId, string OrderNumber, decimal Amount)
        {
            string eventId = Guid.NewGuid().ToString();

            if (merchantOrderCache.TryAdd(Tuple.Create(MerchantId, OrderNumber, eventId), Tuple.Create(DateTime.Now.AddSeconds(5), Amount)))
            {
                return eventId;
            }
            else
            {
                return null;
            }
        }

        public void ReleaseMerchantOrder(string eventId)
        {
            var targetRecord = merchantOrderCache.FirstOrDefault(x => x.Key.Item3 == eventId);
            if (targetRecord.Key != null)
            {
                merchantOrderCache.TryRemove(targetRecord);
            }
        }

        public decimal GetProcessOrderAmount(int MerchantId, string OrderNumber)
        {
            decimal returnAmount = 0;
            try
            {
                houseKeepMerchantOrder(MerchantId);
                returnAmount = merchantOrderCache.Where(x => x.Key.Item1 == MerchantId && x.Key.Item2 != OrderNumber).Sum(x => x.Value.Item2);
            }
            catch (Exception ex)
            {
                NlogLogger.Error("CacheHelper - GetProcessOrderAmount ", ex);
            }

            return returnAmount;
        }

        private void houseKeepMerchantOrder(int MerchantId)
        {
            foreach (var expiredMerchantOrder in merchantOrderCache.Where(x => x.Key.Item1 == MerchantId && x.Value.Item1 < DateTime.Now))
            {
                merchantOrderCache.TryRemove(expiredMerchantOrder);
            }
        }

        #endregion Merchant Cache

        #region Auto Device Cache

        public bool AddAutoPayoutLock(string orderId)
        {
            string eventId = Guid.NewGuid().ToString();
            int currentCount = houseKeepCount;
            houseKeepCount++;

            if (currentCount % 50 == 0)
            {
                HouseKeepExpired();
            }

            if (AutoOrderCache.TryAdd(orderId, DateTime.Now.AddSeconds(15)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void HouseKeepExpired()
        {
            foreach (var expiredPayoutOrder in AutoOrderCache.Where(x => x.Value < DateTime.Now))
            {
                AutoOrderCache.TryRemove(expiredPayoutOrder);
            }
        }

        #endregion Auto Device Cache
    }
}
using Neptune.NsPay.Commons;
using System.Collections.Concurrent;

namespace Neptune.NsPay.Web.TransferApi.SignalR
{
    public class DeviceProcessLock
    {
        private TimeSpan deviceLockKeyExpired = TimeSpan.FromMilliseconds(5000);
        private readonly ConcurrentDictionary<int, Tuple<DateTime, string>> deviceLock = new ConcurrentDictionary<int, Tuple<DateTime, string>>(); // use to lock device from others action 
        private readonly ConcurrentDictionary<int, Tuple<DateTime, string>> deviceProcessLock = new ConcurrentDictionary<int, Tuple<DateTime, string>>(); // use to lock device from others action 
        public DeviceProcessLock()
        {
        }
        public async Task<bool> TryAcquireDeviceLockAsync(int deviceId, string eventId)
        {
            // Check if the lock already exists
            var cDate = DateTime.Now;
            var newValue = Tuple.Create(cDate + deviceLockKeyExpired, eventId);
            var result = deviceLock.AddOrUpdate(deviceId, newValue, (key, oldValue) => (oldValue.Item1 < cDate || oldValue.Item2 == eventId) ? newValue : oldValue);
            var returnResult = (result.Item2 == eventId);
            NlogLogger.Trace("Lock Device - " + deviceId + " " + returnResult);
            return returnResult;
        }

        public void ReleaseLock(int deviceId)
        {
            // Remove the lock from cache
            deviceLock.TryRemove(deviceId, out var _value);
            NlogLogger.Trace("Release Device - " + deviceId + " " + (_value != null));
        }

        #region Device Order Cooldown

        public bool IsDeviceCoolDownOrderProcess(int deviceId)
        {
            bool isLocked = false;
            if (deviceProcessLock.TryGetValue(deviceId, out var _values))
            {
                if (_values.Item1 < DateTime.Now)
                {
                    deviceProcessLock.Remove(deviceId);
                }
                else
                {
                    isLocked = true;
                }
            }

            return !isLocked;
        }

        public bool SetDeviceCoolDownOrderProcess(int deviceId, string orderId)
        {
            TimeSpan coolDownTime = TimeSpan.FromSeconds(5);
            var cDate = DateTime.Now;
            var newValue = Tuple.Create(cDate + coolDownTime, orderId);
            var result = deviceProcessLock.AddOrUpdate(deviceId, newValue, (key, oldValue) => (oldValue.Item1 < cDate || oldValue.Item2 == orderId) ? newValue : oldValue);
            return (result.Item2 == orderId);
        }

        #endregion 
    }
}
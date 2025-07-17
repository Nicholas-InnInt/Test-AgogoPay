using Microsoft.AspNetCore.SignalR;
using Neptune.NsPay.Commons;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SignalRClient;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Newtonsoft.Json;

namespace Neptune.NsPay.Web.TransferApi.SignalR
{

    public class PushNotificationService : IPushNotificationService
    {
        private readonly IHubContext<NotificationHub, INotificationHubClient> _hub;
        private readonly IRedisService _redisService;
        private readonly IWithdrawalDevicesService _withdrawalDevicesService;

        public PushNotificationService(IHubContext<NotificationHub, INotificationHubClient> hub, IRedisService redisService, IWithdrawalDevicesService withdrawalDevicesService)
        {
            _hub = hub;
            _redisService = redisService;
            _withdrawalDevicesService = withdrawalDevicesService;
        }

        public async Task<bool> BalanceChanged(int DeviceId, decimal Balance)
        {
            long versionNumber = TimeHelper.GetUnixTimeStamp(DateTime.Now);
            await _hub.Clients.All.UpdateBalanceTransfer(new BalanceChangedNotification() { DeviceId = DeviceId, Balance = Balance, VersionNumber = versionNumber });
            NlogLogger.Info("BalanceChanged  " + JsonConvert.SerializeObject(new BalanceChangedNotification() { DeviceId = DeviceId, Balance = Balance, VersionNumber = versionNumber }));
            return true;
        }


        public async Task<bool> NotifyAllMerchant()
        {
            long versionNumber = TimeHelper.GetUnixTimeStamp(DateTime.Now);
            var onlineMerchant = NotificationHub.OnlineMerchant();
            NlogLogger.Info("Online Merchant " + JsonConvert.SerializeObject(onlineMerchant));

            if (onlineMerchant.Count > 0)
            {
                var withdrawalList = await GetMerchantDevices(onlineMerchant.Count==1? onlineMerchant.First():null);

                foreach(var merchant in onlineMerchant)
                {
                    await _hub.Clients.Group(merchant).WithdrawDeviceChanged( new MerchantDeviceChangedNotification() {  MerchantCode = merchant , VersionNumber = versionNumber , WithdrawalDevices = withdrawalList.Where(x => x.MerchantCode == merchant).ToList() } );
                }
            }

            return true;
        }

        public async Task<bool> NotifyMerchant(string MerchantCode)
        {
            long versionNumber = TimeHelper.GetUnixTimeStamp(DateTime.Now);
            var withdrawalList = await GetMerchantDevices(MerchantCode);
            await _hub.Clients.Group(MerchantCode).WithdrawDeviceChanged(new MerchantDeviceChangedNotification() { MerchantCode = MerchantCode, VersionNumber = versionNumber, WithdrawalDevices = withdrawalList });
            return true;
        }


        public async Task<List<WithdrawalDeviceRedisModel>> GetMerchantDevices(string merchantCode = "")
        {
            List<WithdrawalDeviceRedisModel> withdrawList = new List<WithdrawalDeviceRedisModel>();
            var list = _withdrawalDevicesService.GetAll().Where(r => (string.IsNullOrEmpty(merchantCode)?true: r.MerchantCode == merchantCode) &&   r.IsDeleted == false);
            foreach (var device in list)
            {
                var balanceInfo = _redisService.GetWitdrawDeviceBalance(device.Id);
                WithdrawalDeviceRedisModel redisModel = new WithdrawalDeviceRedisModel()
                {
                    Id = device.Id,
                    MerchantCode = device.MerchantCode,
                    Name = device.Name,
                    Phone = device.Phone,
                    BankOtp = device.BankOtp,
                    BankType = device.BankType,
                    CardName = device.CardName,
                    LoginPassWord = device.LoginPassWord,
                    Process = device.Process,
                    Status = true,
                    DeviceAdbName = device.DeviceAdbName,
                    Balance = balanceInfo == null ? 0 : balanceInfo.Balance
                };
                withdrawList.Add(redisModel);
            }

            return withdrawList;
        }
        public async Task<bool> IdentifyAndNotifyDeviceChanged(WithdrawalDeviceChangedDto ChangesDetails)
        {
            var notifyMerchant = new List<string>();
            long versionNumber = TimeHelper.GetUnixTimeStamp(DateTime.Now);
            var onlineMerchant = NotificationHub.OnlineMerchant();

            foreach (var device in ChangesDetails.WithdrawalDevice)
            {
                if (device.NewData != null && !notifyMerchant.Contains(device.NewData.MerchantCode))
                {
                    notifyMerchant.Add(device.NewData.MerchantCode);
                }

                if (device.OldData != null && !notifyMerchant.Contains(device.OldData.MerchantCode))
                {
                    notifyMerchant.Add(device.OldData.MerchantCode);
                }
            }

            NlogLogger.Info("Online Merchant " + JsonConvert.SerializeObject(onlineMerchant) + " Notify Merchant " + JsonConvert.SerializeObject(notifyMerchant));

            var finalMerchant = notifyMerchant.Join(onlineMerchant, t1 => t1, t2 => t2, (t1, t2) => t1);

            if (finalMerchant.Count() > 0)
            {
                var merchantDevice = await GetMerchantDevices((finalMerchant.Count() == 1 ? finalMerchant.First() : null));

                foreach (var merchant in finalMerchant)
                {
                    await _hub.Clients.Group(merchant).WithdrawDeviceChanged(new MerchantDeviceChangedNotification() { MerchantCode = merchant, VersionNumber = versionNumber, WithdrawalDevices = merchantDevice.Where(x => x.MerchantCode == merchant).ToList() });
                }
            }
            return true;
        }
    }

    public interface IPushNotificationService
    {
        Task<bool> BalanceChanged(int DeviceId, decimal Balance);
        Task<bool> NotifyAllMerchant();
        Task<bool> NotifyMerchant(string MerchantCode);

        Task<bool> IdentifyAndNotifyDeviceChanged(WithdrawalDeviceChangedDto ChangesDetails);
    }
}

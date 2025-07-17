using Neptune.NsPay.Web.TransferApi.Models;
using Org.BouncyCastle.Asn1.X9;

namespace Neptune.NsPay.Web.TransferApi.SignalR
{
    public class PushOrderHelper
    {
        private readonly PushOrderService _pushOrderService;

        public PushOrderHelper(PushOrderService pushOrderService )
        {
            _pushOrderService = pushOrderService;
        }
        public void DeviceOrderInProcess(string orderId, int deviceId)
        {
            _pushOrderService.deviceOrderInProcess(orderId, deviceId);
        }


        public async Task DeviceOrderCompleted(string orderId, int deviceId)
        {
            await _pushOrderService.deviceOrderCompleted(deviceId, orderId);
        }


        public void DeviceInactiveTracked(int deviceId)
        {
            _pushOrderService.deviceInactiveTracked(deviceId);
        }

        public void DeviceBalanceUpdated(int deviceId)
        {
            _pushOrderService.deviceBalanceUpdated(deviceId);
        }

        public void DeviceStatusUpdate(int deviceId , DeviceProcessStatusEnum currentStatus)
        {
            _pushOrderService.deviceStatusUpdate(deviceId, currentStatus);
        }
            
        public void SendDeviceCurrentOrder(int deviceId)
        {
            _pushOrderService.sendDeviceCurrentOrder(deviceId);
        }

        public async Task SaveDisconnectedDeviceOrder(int deviceId)
        {
            await _pushOrderService.saveDisconnectedDeviceOrder(deviceId);
        }

        public async Task ForceCompleteOrder(int deviceId , string orderId)
        {
            await _pushOrderService.ForceCompleteDeviceOrder(deviceId , orderId);
        }

    }
}

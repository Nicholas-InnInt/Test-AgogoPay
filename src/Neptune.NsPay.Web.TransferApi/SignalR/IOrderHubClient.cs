namespace Neptune.NsPay.Web.TransferApi.SignalR
{
    public interface IOrderHubClient
    {
        Task ProcessOrder(SignalROrderInfo order);

        Task RequestDeviceStatus(int deviceId);

        Task SendMessage(string user, string message);

        Task RequestUpdateBalance(int deviceId);
        Task DisconnectClient(SignalRConnection connection);
    }


    public class SignalRConnection
    {
        public string ConnectionId { get; set; }
        public int DeviceId { get; set; }
    }

    public class SignalROrderInfo
    {
        public string OrderId { get; set; }

        public string OrderNo { get; set; }

        public string QRImageBase64 { get; set; }

        public decimal Amount { get; set; }

        public string RecipientAccountNumber { get; set; }

        public string RecipientAccountHolderName { get; set; }
        public int DeviceId { get; set; } 
        public long timeStamp { get; set; } // push Date in TimeStamp

        public bool IsAccountNameVerified { get; set; } // push Date in TimeStamp

        public string QRContent { get; set; } // push Date in TimeStamp

    }
}

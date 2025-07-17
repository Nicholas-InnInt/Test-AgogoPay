using Neptune.NsPay.PayOrders.Dtos;
using System.Threading.Tasks;

namespace Neptune.NsPay.Web.SignalR
{
    public interface IOrderTrackerHubClient
    {
        Task ReceiveMessage(string message);

        Task PayOrderChanged(PayOrderChangeSignalR orderData);

    }

    public class PayOrderChangeSignalR
    {
        public GetPayOrderForViewDto Order { get; set; }
        public long VersionNumber { get; set; }

        public int ChangeType { get; set; } // 1 is Add , 2 Is Update
    }
}

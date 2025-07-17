using System.Threading.Tasks;

namespace Neptune.NsPay.Web.SignalR.MerchantBalance
{
    public interface IMerchantHubClient
    {

        Task MerchantInfoUpdate(MerchantSignalRContent orderData);

    }
}

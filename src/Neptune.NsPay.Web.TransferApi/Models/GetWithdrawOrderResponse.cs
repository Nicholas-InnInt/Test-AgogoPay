using Neptune.NsPay.RedisExtensions.Models;

namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class GetWithdrawOrderResponse: WithdrawalOrderRedisModel
    {
        public int DeviceId { get; set; }
    }
}

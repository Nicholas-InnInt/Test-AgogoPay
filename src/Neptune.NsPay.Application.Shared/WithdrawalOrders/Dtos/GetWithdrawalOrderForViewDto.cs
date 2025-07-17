using Neptune.NsPay.WithdrawalDevices.Dtos;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class GetWithdrawalOrderForViewDto
    {
        public WithdrawalOrderDto WithdrawalOrder { get; set; }
        public string MerchantName { get; set; }
        public WithdrawalDeviceDto WithdrawalDevice { get; set; }
    }
}
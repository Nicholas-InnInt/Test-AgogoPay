namespace Neptune.NsPay.WithdrawalDevices.Dtos
{
    public class GetWithdrawalDeviceForViewDto
    {
        public WithdrawalDeviceDto WithdrawalDevice { get; set; }
        public string MerchantName { get; set; }
    }
}
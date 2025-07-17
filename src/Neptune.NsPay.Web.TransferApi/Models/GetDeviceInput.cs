using Neptune.NsPay.WithdrawalDevices;

namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class GetDeviceInput
    {
        public string MerchantCode { get; set; }
        public string Phone { get; set; }
        public WithdrawalDevicesBankTypeEnum BankType { get; set; }
    }
}

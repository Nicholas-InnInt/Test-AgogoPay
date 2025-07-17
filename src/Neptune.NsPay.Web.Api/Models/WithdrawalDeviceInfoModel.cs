using Neptune.NsPay.WithdrawalDevices;

namespace Neptune.NsPay.Web.Api.Models
{
    public class WithdrawalDeviceInfoModel
    {
        public int Id { get; set; }
        public string MerchantCode { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string CardNumber { get; set; }
        public WithdrawalDevicesBankTypeEnum BankType { get; set; }
        public string BankOtp { get; set; }
        public string LoginPassWord { get; set; }
        public string CardName { get; set; }
        public WithdrawalDevicesProcessTypeEnum Process { get; set; }
        public bool Status { get; set; }
        public string DeviceAdbName { get; set; }
        public decimal MinMoney { get; set; }
        public decimal MaxMoney { get; set; }
        public decimal Balance { get; set; }
        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }
    }
}
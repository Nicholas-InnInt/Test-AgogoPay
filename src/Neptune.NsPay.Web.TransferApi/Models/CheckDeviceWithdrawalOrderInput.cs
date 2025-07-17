namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class CheckDeviceWithdrawalOrderInput
    {
        public int DeviceId { get; set; }
        public string OrderId { get; set; }

        public bool? AutoCompleteOrder { get; set; }
    }
}
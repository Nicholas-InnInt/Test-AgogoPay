namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class UpdateBalanceInput
    {
        public int DeviceId { get; set; }
        public decimal Balance { get; set; }
    }
}

namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class UpdateOrderStatusInput
    {
        public int DeviceId { get; set; }
        public string? OrderId { get; set; }
        //0未使用，1使用
        public int OrderStatus { get; set; }
    }
}

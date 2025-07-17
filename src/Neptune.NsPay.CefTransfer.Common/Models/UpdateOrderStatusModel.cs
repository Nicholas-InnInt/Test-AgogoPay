namespace Neptune.NsPay.CefTransfer.Common.Models
{
    public class UpdateOrderStatusModel
    {
        public int DeviceId { get; set; }
        public string OrderId { get; set; }
        //0未使用，1使用
        public int OrderStatus { get; set; }
    }
}
namespace Neptune.NsPay.Web.TransferApi.Models
{
    public class UpdateProcessDeviceStatusInput
    {
        public int DeviceId { get; set; }
        public string? OrderId { get; set; }
        public string? Remark { get; set; }
        public DeviceProcessStatusEnum CurrentStatus { get; set; }
    }

    public enum DeviceProcessStatusEnum
    {
        Idle = 1,
        ProcessPayout = 2,
    }
}

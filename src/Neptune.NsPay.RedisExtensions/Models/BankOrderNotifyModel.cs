using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class BankOrderNotifyModel
    {
        public PayMentTypeEnum Type { get; set; }
        public string? RefNo { get; set; }
        public int PayMentId { get; set; }
        //public string? Phone { get; set; }
        //public string? CardNo { get; set; }
        public string? Remark { get; set; }
        public DateTime TransferTime { get; set; }
        public decimal Money { get; set; }
        public DateTime Createtime { get; set; }
    }
}

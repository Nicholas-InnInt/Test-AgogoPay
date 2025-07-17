using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.Web.PayMonitorApi.Models.SignalR
{
    public class ListenMerchantData
    {
        public string Name { get; set; }

        public string MerchantCode { get; set; }

        public int PayGroupId { get; set; }

        public bool IsDeleted { get; set; }

        public override bool Equals(object obj)
        {
            // Check if the object is the same instance
            if (ReferenceEquals(this, obj)) return true;

            // Check if the object is of the same type
            if (obj == null || GetType() != obj.GetType()) return false;

            var other = (ListenMerchantData)obj;

            // Compare properties for equality
            return Name == other.Name && MerchantCode == other.MerchantCode && PayGroupId == other.PayGroupId && IsDeleted == other.IsDeleted;
        }

        // Override GetHashCode method
        public override int GetHashCode()
        {
            return Name?.GetHashCode() ^ MerchantCode?.GetHashCode() ^ PayGroupId.GetHashCode() ^ IsDeleted.GetHashCode() ?? 0;
        }

    }

    public class ListenPaymentData
    {
        public string Name { get; set; }

        public PayMentTypeEnum Type { get; set; }
        public string Phone { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public string PassWord { get; set; }
        public PayMentStatusEnum Status { get; set; }

        public bool PayMentStatus { get; set; }

        public bool UseMoMo { get; set; }
        public bool IsDeleted { get; set; }

        public decimal MinMoney { get; set; }
        public decimal MaxMoney { get; set; }
        public decimal LimitMoney { get; set; }
        public decimal BalanceLimitMoney { get; set; }
        public decimal MoMoRate { get; set; }
        public decimal ZaloRate { get; set; }
        public decimal VittelPayRate { get; set; }


        public override bool Equals(object obj)
        {
            // Check if the object is the same instance
            if (ReferenceEquals(this, obj)) return true;

            // Check if the object is of the same type
            if (obj == null || GetType() != obj.GetType()) return false;

            var other = (ListenPaymentData)obj;


            // Compare properties for equality
            return
                Name == other.Name && Type == other.Type && Phone == other.Phone && CardNumber == other.CardNumber
                && FullName == other.FullName && PassWord == other.PassWord && Status == other.Status
                && PayMentStatus == other.PayMentStatus && UseMoMo == other.UseMoMo && MinMoney == other.MinMoney
                 && MaxMoney == other.MaxMoney && LimitMoney == other.LimitMoney && BalanceLimitMoney == other.BalanceLimitMoney
                  && MoMoRate == other.MoMoRate && ZaloRate == other.ZaloRate && VittelPayRate == other.VittelPayRate
                && IsDeleted == other.IsDeleted;
        }

        // Override GetHashCode method
        public override int GetHashCode()
        {
            return Name?.GetHashCode() ^ Type.GetHashCode() ^ Phone?.GetHashCode() ^ CardNumber?.GetHashCode() ^ FullName?.GetHashCode() ^ PassWord?.GetHashCode()
                ^ Status.GetHashCode() ^ PayMentStatus.GetHashCode() ^ UseMoMo.GetHashCode() ^ MinMoney.GetHashCode() ^ MaxMoney.GetHashCode() ^ LimitMoney.GetHashCode()
                 ^ BalanceLimitMoney.GetHashCode() ^ MoMoRate.GetHashCode() ^ ZaloRate.GetHashCode() ^ VittelPayRate.GetHashCode() ^ IsDeleted.GetHashCode() ?? 0;
        }

    }

    public class ListenPayGroupMentData
    {
        public int GroupId { get; set; }

        public int PayMentId { get; set; }

        public bool Status { get; set; }

        public override bool Equals(object obj)
        {
            // Check if the object is the same instance
            if (ReferenceEquals(this, obj)) return true;

            // Check if the object is of the same type
            if (obj == null || GetType() != obj.GetType()) return false;

            var other = (ListenPayGroupMentData)obj;


            // Compare properties for equality
            return GroupId == other.GroupId && PayMentId == other.PayMentId && Status == other.Status;
        }

        // Override GetHashCode method
        public override int GetHashCode()
        {
            return GroupId.GetHashCode() ^ PayMentId.GetHashCode() ^ Status.GetHashCode();
        }

    }
}

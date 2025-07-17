using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace Neptune.NsPay.MerchantBills
{
    [Table("MerchantBills")]
    public class MerchantBill : Entity<long>
    {
        [StringLength(MerchantBillConsts.MaxMerchantCodeLength, MinimumLength = MerchantBillConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        public virtual int MerchantId { get; set; }

        [StringLength(MerchantBillConsts.MaxBillNoLength, MinimumLength = MerchantBillConsts.MinBillNoLength)]
        public virtual string BillNo { get; set; }

        public virtual MerchantBillTypeEnum BillType { get; set; }

        public virtual decimal Money { get; set; }

        public virtual decimal Rate { get; set; }

        public virtual decimal FeeMoney { get; set; }

        public virtual decimal BalanceBefore { get; set; }

        public virtual decimal BalanceAfter { get; set; }

        [StringLength(MerchantBillConsts.MaxPlatformCodeLength, MinimumLength = MerchantBillConsts.MinPlatformCodeLength)]
        public virtual string PlatformCode { get; set; }

        public virtual long OrderId { get; set; }

        public virtual long CreationUnixTime { get; set; }

        public virtual DateTime CreationTime { get; set; }

    }
}
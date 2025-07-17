using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neptune.NsPay.MerchantRates
{
    [Table("MerchantRates")]
    public class MerchantRate : FullAuditedEntity
    {
        [Required]
        [StringLength(MerchantRateConsts.MaxMerchantCodeLength, MinimumLength = MerchantRateConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        public virtual int MerchantId { get; set; }

        public virtual decimal ScanBankRate { get; set; }

        public virtual decimal ScratchCardRate { get; set; }

        public virtual decimal MoMoRate { get; set; }

        public virtual decimal USDTFixedFees { get; set; }

        public virtual decimal USDTRateFees { get; set; }
    }
}
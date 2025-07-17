using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neptune.NsPay.PayMents
{
    [Table("PayMents")]
    public class PayMent : FullAuditedEntity
    {
        [StringLength(PayMentConsts.MaxNameLength, MinimumLength = PayMentConsts.MinNameLength)]
        public virtual string Name { get; set; }

        public virtual PayMentTypeEnum Type { get; set; }

        public virtual PayMentCompanyTypeEnum CompanyType { get; set; }

        [StringLength(PayMentConsts.MaxGatewayLength, MinimumLength = PayMentConsts.MinGatewayLength)]
        public virtual string Gateway { get; set; }

        [StringLength(PayMentConsts.MaxCompanyKeyLength, MinimumLength = PayMentConsts.MinCompanyKeyLength)]
        public virtual string CompanyKey { get; set; }

        [StringLength(PayMentConsts.MaxCompanySecretLength, MinimumLength = PayMentConsts.MinCompanySecretLength)]
        public virtual string CompanySecret { get; set; }

        public virtual bool BusinessType { get; set; }

        [StringLength(PayMentConsts.MaxFullNameLength, MinimumLength = PayMentConsts.MinFullNameLength)]
        public virtual string FullName { get; set; }

        [StringLength(PayMentConsts.MaxPhoneLength, MinimumLength = PayMentConsts.MinPhoneLength)]
        public virtual string Phone { get; set; }

        [StringLength(PayMentConsts.MaxMailLength, MinimumLength = PayMentConsts.MinMailLength)]
        public virtual string Mail { get; set; }

        [StringLength(PayMentConsts.MaxQrCodeLength, MinimumLength = PayMentConsts.MinQrCodeLength)]
        public virtual string QrCode { get; set; }

        [StringLength(PayMentConsts.MaxPassWordLength, MinimumLength = PayMentConsts.MinPassWordLength)]
        public virtual string PassWord { get; set; }

        [StringLength(PayMentConsts.MaxCardNumberLength, MinimumLength = PayMentConsts.MinCardNumberLength)]
        public virtual string CardNumber { get; set; }

        [StringLength(PayMentConsts.MaxMoMoCheckSumLength, MinimumLength = PayMentConsts.MinMoMoCheckSumLength)]
        public virtual string MoMoCheckSum { get; set; }

        [StringLength(PayMentConsts.MaxMoMoPHashLength, MinimumLength = PayMentConsts.MinMoMoPHashLength)]
        public virtual string MoMoPHash { get; set; }

        public virtual decimal MinMoney { get; set; }

        public virtual decimal MaxMoney { get; set; }

        public virtual decimal LimitMoney { get; set; }

        public virtual decimal BalanceLimitMoney { get; set; }

        public virtual bool UseMoMo { get; set; }

        public virtual PayMentDispensEnum DispenseType { get; set; }

        [StringLength(PayMentConsts.MaxRemarkLength, MinimumLength = PayMentConsts.MinRemarkLength)]
        public virtual string Remark { get; set; }

        public virtual PayMentStatusEnum Status { get; set; }

        public virtual decimal MoMoRate { get; set; }
        public virtual decimal ZaloRate { get; set; }
        public virtual decimal VittelPayRate { get; set; }
        public string CryptoWalletAddress { get; set; }
        public decimal? CryptoMinConversionRate { get; set; }
        public decimal? CryptoMaxConversionRate { get; set; }
    }
}
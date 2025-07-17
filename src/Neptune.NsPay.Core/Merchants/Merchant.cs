using Abp.Domain.Entities.Auditing;
using Neptune.NsPay.Utils;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neptune.NsPay.Merchants
{
    [Table("Merchants")]
    public class Merchant : FullAuditedEntity
    {
        [Required]
        [StringLength(MerchantConsts.MaxNameLength, MinimumLength = MerchantConsts.MinNameLength)]
        public virtual string Name { get; set; }

        [StringLength(MerchantConsts.MaxMailLength, MinimumLength = MerchantConsts.MinMailLength)]
        public virtual string Mail { get; set; }

        [StringLength(MerchantConsts.MaxPhoneLength, MinimumLength = MerchantConsts.MinPhoneLength)]
        public virtual string Phone { get; set; }

        [StringLength(MerchantConsts.MaxMerchantCodeLength, MinimumLength = MerchantConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        [StringLength(MerchantConsts.MaxMerchantSecretLength, MinimumLength = MerchantConsts.MinMerchantSecretLength)]
        public virtual string MerchantSecret { get; set; }

        [StringLength(MerchantConsts.MaxPlatformCodeLength, MinimumLength = MerchantConsts.MinPlatformCodeLength)]
        public virtual string PlatformCode { get; set; }

        public virtual int PayGroupId { get; set; }

        [StringLength(MerchantConsts.MaxCountryTypeLength, MinimumLength = MerchantConsts.MinCountryTypeLength)]
        public virtual string CountryType { get; set; }

        [StringLength(MerchantConsts.MaxRemarkLength, MinimumLength = MerchantConsts.MinRemarkLength)]
        public virtual string Remark { get; set; }

        public virtual MerchantTypeEnum MerchantType { get; set; }

        public virtual CountryCodeEnum PhoneCountryCode { get; set; }
    }
}
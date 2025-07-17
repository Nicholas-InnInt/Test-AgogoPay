using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Neptune.NsPay.PayGroups
{
    [Table("PayGroups")]
    public class PayGroup : FullAuditedEntity
    {

        [StringLength(PayGroupConsts.MaxGroupNameLength, MinimumLength = PayGroupConsts.MinGroupNameLength)]
        public virtual string GroupName { get; set; }

        [StringLength(PayGroupConsts.MaxBankApiLength, MinimumLength = PayGroupConsts.MinBankApiLength)]
        public virtual string BankApi { get; set; }

        [StringLength(PayGroupConsts.MaxVietcomApiLength, MinimumLength = PayGroupConsts.MinVietcomApiLength)]
        public virtual string VietcomApi { get; set; }

        public virtual bool Status { get; set; }

    }
}
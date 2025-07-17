using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Neptune.NsPay.NsPaySystemSettings
{
    [Table("NsPaySystemSettings")]
    public class NsPaySystemSetting : FullAuditedEntity
    {

        [StringLength(NsPaySystemSettingConsts.MaxKeyLength, MinimumLength = NsPaySystemSettingConsts.MinKeyLength)]
        public virtual string Key { get; set; }

        [MinLength(NsPaySystemSettingConsts.MinValueLength)]
        public virtual string Value { get; set; }

    }
}
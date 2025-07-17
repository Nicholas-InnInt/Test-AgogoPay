using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace Neptune.NsPay.NsPayBackgroundJobs
{
    [Table("NsPayBackgroundJobs")]
    public class NsPayBackgroundJob : FullAuditedEntity<Guid>
    {

        [StringLength(NsPayBackgroundJobConsts.MaxNameLength, MinimumLength = NsPayBackgroundJobConsts.MinNameLength)]
        public virtual string Name { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxGroupNameLength, MinimumLength = NsPayBackgroundJobConsts.MinGroupNameLength)]
        public virtual string GroupName { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxCronLength, MinimumLength = NsPayBackgroundJobConsts.MinCronLength)]
        public virtual string Cron { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxApiUrlLength, MinimumLength = NsPayBackgroundJobConsts.MinApiUrlLength)]
        public virtual string ApiUrl { get; set; }

        public virtual NsPayBackgroundJobRequsetModeEnum RequsetMode { get; set; }

        public virtual NsPayBackgroundJobStateEnum State { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxParamDataLength, MinimumLength = NsPayBackgroundJobConsts.MinParamDataLength)]
        public virtual string ParamData { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxMerchantCodeLength, MinimumLength = NsPayBackgroundJobConsts.MinMerchantCodeLength)]
        public virtual string MerchantCode { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxDescriptionLength, MinimumLength = NsPayBackgroundJobConsts.MinDescriptionLength)]
        public virtual string Description { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxRemarkLength, MinimumLength = NsPayBackgroundJobConsts.MinRemarkLength)]
        public virtual string Remark { get; set; }

        public virtual bool IsPaused { get; set; }
        public virtual bool IsRestart { get; set; }
    }
}
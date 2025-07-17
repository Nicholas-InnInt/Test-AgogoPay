using Neptune.NsPay.NsPayBackgroundJobs;

using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.NsPayBackgroundJobs.Dtos
{
    public class CreateOrEditNsPayBackgroundJobDto : EntityDto<Guid?>
    {

        [StringLength(NsPayBackgroundJobConsts.MaxNameLength, MinimumLength = NsPayBackgroundJobConsts.MinNameLength)]
        public string Name { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxGroupNameLength, MinimumLength = NsPayBackgroundJobConsts.MinGroupNameLength)]
        public string GroupName { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxCronLength, MinimumLength = NsPayBackgroundJobConsts.MinCronLength)]
        public string Cron { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxApiUrlLength, MinimumLength = NsPayBackgroundJobConsts.MinApiUrlLength)]
        public string ApiUrl { get; set; }

        public NsPayBackgroundJobRequsetModeEnum RequsetMode { get; set; }

        public NsPayBackgroundJobStateEnum State { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxParamDataLength, MinimumLength = NsPayBackgroundJobConsts.MinParamDataLength)]
        public string ParamData { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxMerchantCodeLength, MinimumLength = NsPayBackgroundJobConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxDescriptionLength, MinimumLength = NsPayBackgroundJobConsts.MinDescriptionLength)]
        public string Description { get; set; }

        [StringLength(NsPayBackgroundJobConsts.MaxRemarkLength, MinimumLength = NsPayBackgroundJobConsts.MinRemarkLength)]
        public string Remark { get; set; }

    }
}
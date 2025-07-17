using Neptune.NsPay.NsPayBackgroundJobs;

using System;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.NsPayBackgroundJobs.Dtos
{
    public class NsPayBackgroundJobDto : EntityDto<Guid>
    {
        public string Name { get; set; }

        public string GroupName { get; set; }

        public string Cron { get; set; }

        public string ApiUrl { get; set; }

        public NsPayBackgroundJobRequsetModeEnum RequsetMode { get; set; }

        public NsPayBackgroundJobStateEnum State { get; set; }

        public string ParamData { get; set; }

        public string MerchantCode { get; set; }

        public string Description { get; set; }

        public string Remark { get; set; }

    }
}
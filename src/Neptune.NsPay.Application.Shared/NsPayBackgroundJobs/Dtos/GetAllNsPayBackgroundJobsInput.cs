using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.NsPayBackgroundJobs.Dtos
{
    public class GetAllNsPayBackgroundJobsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string NameFilter { get; set; }

        public string CronFilter { get; set; }

        public string ApiUrlFilter { get; set; }

        public int? RequsetModeFilter { get; set; }

        public int? StateFilter { get; set; }

        public string ParamDataFilter { get; set; }

        public string MerchantCodeFilter { get; set; }

        public string DescriptionFilter { get; set; }

        public string RemarkFilter { get; set; }

    }
}
using Neptune.NsPay.NsPayBackgroundJobs.Dtos;

using Abp.Extensions;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.NsPayBackgroundJobs
{
    public class CreateOrEditNsPayBackgroundJobModalViewModel
    {
        public CreateOrEditNsPayBackgroundJobDto NsPayBackgroundJob { get; set; }

        public bool IsEditMode => NsPayBackgroundJob.Id.HasValue;
    }
}
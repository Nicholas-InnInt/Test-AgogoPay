using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.NsPayBackgroundJobs.Dtos
{
    public class GetNsPayBackgroundJobForEditOutput
    {
        public CreateOrEditNsPayBackgroundJobDto NsPayBackgroundJob { get; set; }

    }
}
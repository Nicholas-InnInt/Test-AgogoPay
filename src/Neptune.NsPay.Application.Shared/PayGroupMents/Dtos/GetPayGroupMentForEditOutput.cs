using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.PayGroupMents.Dtos
{
    public class GetPayGroupMentForEditOutput
    {
        public CreateOrEditPayGroupMentDto PayGroupMent { get; set; }

    }
}
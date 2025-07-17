using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Neptune.NsPay.PayGroupMents.Dtos
{
    public class CreateOrEditPayGroupMentDto : EntityDto<int?>
    {

        public int GroupId { get; set; }
        public string GroupName { get; set; }

        public int PayMentId { get; set; }

        public bool Status { get; set; }

        public string PayMentName { get; set; }

        public List<int> PayMentIds { get; set; }

    }
}
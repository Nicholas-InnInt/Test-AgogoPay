using System;
using Abp.Application.Services.Dto;
using Neptune.NsPay.PayMents;

namespace Neptune.NsPay.PayGroupMents.Dtos
{
    public class PayGroupMentDto : EntityDto
    {
        public int GroupId { get; set; }

        public string PayMentName { get; set; }

        public PayMentTypeEnum PayMentType { get; set; }

        public int PayMentId { get; set; }

        public bool Status { get; set; }

    }
}
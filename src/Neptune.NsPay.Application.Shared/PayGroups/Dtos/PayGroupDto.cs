using System;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.PayGroups.Dtos
{
    public class PayGroupDto : EntityDto
    {
        public string GroupName { get; set; }

        public string BankApi { get; set; }

        public string VietcomApi { get; set; }

        public bool Status { get; set; }

    }
}
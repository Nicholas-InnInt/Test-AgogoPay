using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.PayGroups.Dtos
{
    public class GetPayGroupForEditOutput
    {
        public CreateOrEditPayGroupDto PayGroup { get; set; }

    }
}
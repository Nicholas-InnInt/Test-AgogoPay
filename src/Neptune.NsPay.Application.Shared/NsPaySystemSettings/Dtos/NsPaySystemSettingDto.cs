using System;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.NsPaySystemSettings.Dtos
{
    public class NsPaySystemSettingDto : EntityDto
    {
        public string Key { get; set; }

        public string Value { get; set; }

    }
}
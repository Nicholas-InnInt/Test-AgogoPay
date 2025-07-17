using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.NsPaySystemSettings.Dtos
{
    public class GetNsPaySystemSettingForEditOutput
    {
        public CreateOrEditNsPaySystemSettingDto NsPaySystemSetting { get; set; }

    }
}
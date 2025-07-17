using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.NsPaySystemSettings.Dtos
{
    public class CreateOrEditNsPaySystemSettingDto : EntityDto<int?>
    {

        [StringLength(NsPaySystemSettingConsts.MaxKeyLength, MinimumLength = NsPaySystemSettingConsts.MinKeyLength)]
        public string Key { get; set; }

        [MinLength(NsPaySystemSettingConsts.MinValueLength)]
        public string Value { get; set; }

    }
}
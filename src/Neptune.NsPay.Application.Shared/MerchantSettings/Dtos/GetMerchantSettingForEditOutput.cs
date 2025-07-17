using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantSettings.Dtos
{
    public class GetMerchantSettingForEditOutput
    {
        public CreateOrEditMerchantSettingDto MerchantSetting { get; set; }

    }
}
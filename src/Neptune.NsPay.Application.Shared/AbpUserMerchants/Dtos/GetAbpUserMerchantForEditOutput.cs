using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.AbpUserMerchants.Dtos
{
    public class GetAbpUserMerchantForEditOutput
    {
        public CreateOrEditAbpUserMerchantDto AbpUserMerchant { get; set; }

    }
}
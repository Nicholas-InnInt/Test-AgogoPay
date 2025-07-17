using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantRates.Dtos
{
    public class GetMerchantRateForEditOutput
    {
        public CreateOrEditMerchantRateDto MerchantRate { get; set; }

    }
}
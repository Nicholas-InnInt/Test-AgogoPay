using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.Merchants.Dtos
{
    public class GetMerchantForEditOutput
    {
        public CreateOrEditMerchantDto Merchant { get; set; }

    }
}
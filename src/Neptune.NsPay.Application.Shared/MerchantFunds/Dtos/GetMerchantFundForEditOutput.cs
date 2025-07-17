using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantFunds.Dtos
{
    public class GetMerchantFundForEditOutput
    {
        public CreateOrEditMerchantFundDto MerchantFund { get; set; }

    }
}
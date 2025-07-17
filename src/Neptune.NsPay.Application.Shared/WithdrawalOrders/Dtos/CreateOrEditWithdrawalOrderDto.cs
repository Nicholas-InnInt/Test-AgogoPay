using Neptune.NsPay.WithdrawalOrders;

using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class CreateOrEditWithdrawalOrderDto : EntityDto<string>
    {

        [StringLength(WithdrawalOrderConsts.MaxMerchantCodeLength, MinimumLength = WithdrawalOrderConsts.MinMerchantCodeLength)]
        public string MerchantCode { get; set; }

    }
}
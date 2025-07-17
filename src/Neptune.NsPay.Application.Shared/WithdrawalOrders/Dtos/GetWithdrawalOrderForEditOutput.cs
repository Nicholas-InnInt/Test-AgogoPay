using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class GetWithdrawalOrderForEditOutput
    {
        public CreateOrEditWithdrawalOrderDto WithdrawalOrder { get; set; }

    }
}
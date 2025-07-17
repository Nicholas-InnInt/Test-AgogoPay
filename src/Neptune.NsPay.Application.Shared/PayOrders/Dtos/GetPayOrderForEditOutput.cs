using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.PayOrders.Dtos
{
    public class GetPayOrderForEditOutput
    {
        public CreateOrEditPayOrderDto PayOrder { get; set; }

    }
}
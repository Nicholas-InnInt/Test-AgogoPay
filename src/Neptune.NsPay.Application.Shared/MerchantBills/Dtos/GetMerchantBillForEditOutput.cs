using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.MerchantBills.Dtos
{
    public class GetMerchantBillForEditOutput
    {
        public CreateOrEditMerchantBillDto MerchantBill { get; set; }

    }
}
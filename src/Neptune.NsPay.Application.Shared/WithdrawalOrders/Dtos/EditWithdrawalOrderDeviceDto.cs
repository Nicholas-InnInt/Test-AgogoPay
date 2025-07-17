using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;

namespace Neptune.NsPay.WithdrawalOrders.Dtos
{
    public class EditWithdrawalOrderDeviceDto
    {
        public string MerchantCode { get; set; }
        public List<string> WithdrawalIds { get; set; }
        public List<string> OrderNos { get; set; }
        public int DeviceId { get; set; }

    }
}
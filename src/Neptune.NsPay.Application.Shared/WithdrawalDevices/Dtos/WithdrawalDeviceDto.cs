using Neptune.NsPay.WithdrawalDevices;

using System;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.WithdrawalDevices.Dtos
{
    public class WithdrawalDeviceDto : EntityDto
    {
        public string Name { get; set; }

        public string Phone { get; set; }

        public WithdrawalDevicesBankTypeEnum BankType { get; set; }

        public bool Status { get; set; }

        public string CardName { get; set; }
        public WithdrawalDevicesProcessTypeEnum Process { get; set; }
        public string MerchantCode { get; set; }

        public string Balance { get; set; }
        public decimal MinMoney { get; set; }

        public decimal MaxMoney { get; set; }
    }
}
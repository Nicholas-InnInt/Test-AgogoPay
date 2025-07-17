using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Neptune.NsPay.WithdrawalDevices.Dtos
{
    public class GetWithdrawalDeviceForEditOutput
    {
        public CreateOrEditWithdrawalDeviceDto WithdrawalDevice { get; set; }

    }
}
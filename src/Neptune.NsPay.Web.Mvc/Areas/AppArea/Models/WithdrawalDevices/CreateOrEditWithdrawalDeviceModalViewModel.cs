using Neptune.NsPay.WithdrawalDevices.Dtos;

using Abp.Extensions;
using Neptune.NsPay.Merchants.Dtos;
using System.Collections.Generic;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.WithdrawalDevices
{
    public class CreateOrEditWithdrawalDeviceModalViewModel
    {
        public CreateOrEditWithdrawalDeviceDto WithdrawalDevice { get; set; }

        public List<MerchantDto> Merchants { get; set; }

        public bool IsInternalMerchant { get; set; }

        public bool IsEditMode => WithdrawalDevice.Id.HasValue;
    }
}
using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.WithdrawalDevices.Dtos
{
    public class GetAllWithdrawalDevicesInput : PagedAndSortedResultRequestDto
    {
        public string MerchantCodeFilter { get; set; }
        public string Filter { get; set; }

        public string NameFilter { get; set; }

        public string PhoneFilter { get; set; }

        public int? BankTypeFilter { get; set; }

        public int? WithdrawProcessFilter { get; set; }

    }
}
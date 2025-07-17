using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.MerchantWithdrawBanks.Dtos
{
    public class GetAllMerchantWithdrawBanksInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string BankNameFilter { get; set; }

        public string ReceivCardFilter { get; set; }

        public string ReceivNameFilter { get; set; }

    }
}
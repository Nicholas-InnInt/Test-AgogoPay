using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.MerchantWithdraws.Dtos
{
    public class GetAllMerchantWithdrawsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string MerchantCodeFilter { get; set; }

        public string WithDrawNoFilter { get; set; }

        public string BankNameFilter { get; set; }

        public string ReceivCardFilter { get; set; }

        public string ReceivNameFilter { get; set; }

        public int? StatusFilter { get; set; }

        public DateTime? MaxReviewTimeFilter { get; set; }
        public DateTime? MinReviewTimeFilter { get; set; }

    }
}
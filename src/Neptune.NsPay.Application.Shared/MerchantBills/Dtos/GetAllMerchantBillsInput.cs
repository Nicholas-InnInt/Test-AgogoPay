using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.MerchantBills.Dtos
{
    public class GetAllMerchantBillsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public string MerchantCodeFilter { get; set; }
        public string BillNoFilter { get; set; }
        public int? BillTypeFilter { get; set; }
        public string UtcTimeFilter { get; set; } = "GMT7+";
        public DateTime? MinCreationTimeFilter { get; set; }
        public DateTime? MaxCreationTimeFilter { get; set; }
        public DateTime? MinTransactionTimeFilter { get; set; }
        public DateTime? MaxTransactionTimeFilter { get; set; }
    }
}
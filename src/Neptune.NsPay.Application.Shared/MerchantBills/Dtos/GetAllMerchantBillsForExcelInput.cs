using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;

namespace Neptune.NsPay.MerchantBills.Dtos
{
    public class GetAllMerchantBillsForExcelInput
    {
        public string Filter { get; set; }
        public string MerchantCodeFilter { get; set; }
        public string BillNoFilter { get; set; }
        public int? BillTypeFilter { get; set; }
        public string UtcTimeFilter { get; set; } = "GMT7+";
        public DateTime? MinCreationTimeFilter { get; set; }
        public DateTime? MaxCreationTimeFilter { get; set; }
        public List<int> userMerchantIdsList { get; set; }
        public DateTime? MinTransactionTimeFilter { get; set; }
        public DateTime? MaxTransactionTimeFilter { get; set; }

    }
}
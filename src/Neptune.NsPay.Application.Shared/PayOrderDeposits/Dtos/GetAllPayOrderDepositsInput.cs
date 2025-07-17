using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;

namespace Neptune.NsPay.PayOrderDeposits.Dtos
{
    public class GetAllPayOrderDepositsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public string MerchantCodeFilter { get; set; }
        public string RefNoFilter { get; set; }
        public string OrderNoFilter { get; set; }
        public string OrderMarkFilter { get; set; }
        public string UserNameFilter { get; set; }
        public string AccountNoFilter { get; set; }
        public string UserMemberFilter { get; set; }
        public int OrderPayTypeFilter { get; set; }
        public int DepositOrderStatusFilter { get; set; }
        public string BankOrderStatusFilter { get; set; } = "CRDT";
        public DateTime? MaxTransactionTimeFilter { get; set; }
        public DateTime? MinTransactionTimeFilter { get; set; }
        public decimal? MinMoneyFilter { get; set; }
        public decimal? MaxMoneyFilter { get; set; }
        public string UtcTimeFilter { get; set; } = "GMT7+";
        public List<int> MerchantIds { get; set; }

    }
}
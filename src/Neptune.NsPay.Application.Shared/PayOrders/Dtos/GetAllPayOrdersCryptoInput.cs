using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;

namespace Neptune.NsPay.PayOrders.Dtos
{
    public class GetAllPayOrdersCryptoInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public string MerchantCodeFilter { get; set; }
        public string OrderNoFilter { get; set; }
        public string OrderMarkFilter { get; set; }
        public int? OrderPayTypeFilter { get; set; }
        public int? OrderStatusFilter { get; set; }
        public int? OrderBankFilter { get; set; }
        public int? ScoreStatusFilter { get; set; }
        public DateTime? OrderCreationTimeStartDate { get; set; }
        public DateTime? OrderCreationTimeEndDate { get; set; }
        public string UtcTimeFilter { get; set; } = "GMT7+";
        public decimal? MinOrderMoneyFilter { get; set; }
        public decimal? MaxOrderMoneyFilter { get; set; }
        public string OrderId { get; set; }
        public List<int> MerchantIds { get; set; }
    }
}
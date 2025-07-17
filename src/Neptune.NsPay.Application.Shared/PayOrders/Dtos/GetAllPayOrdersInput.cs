using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;

namespace Neptune.NsPay.PayOrders.Dtos
{
    public class GetAllPayOrdersInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public string MerchantCodeFilter { get; set; }
        public string OrderNoFilter { get; set; }
        public string OrderMarkFilter { get; set; }
        public int? OrderPayTypeFilter { get; set; }
        public int? OrderTypeFilter { get; set; }
        public int? OrderStatusFilter { get; set; }
        public int? OrderBankFilter { get; set; }
        public int? ScoreStatusFilter { get; set; }
        public DateTime? OrderCreationTimeStartDate { get; set; }
        public DateTime? OrderCreationTimeEndDate { get; set; }
        public string UtcTimeFilter { get; set; } = "GMT7+";
        public string PayMentPhoneFilter { get; set; }
        public decimal? MinOrderMoneyFilter { get; set; }
        public decimal? MaxOrderMoneyFilter { get; set; }
        public string OrderId { get; set; }
        public List<int> MerchantIds { get; set; }
        public string CardNumberFilter { get; set; }
    }
}
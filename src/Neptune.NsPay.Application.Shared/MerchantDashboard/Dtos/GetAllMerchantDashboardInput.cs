using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.MerchantDashboard.Dtos
{
    public class GetAllMerchantDashboardInput : PagedAndSortedResultRequestDto
    {
        public string MerchantCodeFilter { get; set; }

        public DateTime? OrderCreationTimeStartDate { get; set; }

        public DateTime? OrderCreationTimeEndDate { get; set; }
        public string UtcTimeFilter { get; set; } = "GMT7+";

        public string OrderColumn { get; set; }

        public string OrderDirection { get; set; }

    }
}
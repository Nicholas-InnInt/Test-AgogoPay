using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.PayMents.Dtos
{
    public class GetAllPayMentsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string NameFilter { get; set; }

        public int? TypeFilter { get; set; }

        public int? CompanyTypeFilter { get; set; }

        public string PhoneFilter { get; set; }
        public int? ShowStatusFilter { get; set; }
        public int? UseMoMoFilter { get; set; }
        public int? PayMentStatusFilter { get; set; }

    }
}
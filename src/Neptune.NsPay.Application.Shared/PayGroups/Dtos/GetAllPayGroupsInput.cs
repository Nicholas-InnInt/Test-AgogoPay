using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.PayGroups.Dtos
{
    public class GetAllPayGroupsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string GroupNameFilter { get; set; }

        public string BankApiFilter { get; set; }

        public string VietcomApiFilter { get; set; }

        public int? StatusFilter { get; set; }

    }
}
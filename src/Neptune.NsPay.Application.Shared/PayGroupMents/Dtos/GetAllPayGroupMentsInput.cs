using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.PayGroupMents.Dtos
{
    public class GetAllPayGroupMentsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public int PayGroupsIdFilter { get; set; }

    }
}
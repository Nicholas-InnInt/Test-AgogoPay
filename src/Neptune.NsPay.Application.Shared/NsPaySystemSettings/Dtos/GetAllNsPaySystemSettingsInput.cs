using Abp.Application.Services.Dto;
using System;

namespace Neptune.NsPay.NsPaySystemSettings.Dtos
{
    public class GetAllNsPaySystemSettingsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string KeyFilter { get; set; }

        public string ValueFilter { get; set; }

    }
}

using Neptune.NsPay.Caching.Dto;
using System.Collections.Generic;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.NsPayMaintenance
{
    public class NsPayMaintenanceViewModel
    {
        public IReadOnlyList<CacheDto> Caches { get; set; }
        public bool CanClearAllCaches { get; set; }
    }
}
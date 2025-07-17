using System.Collections.Generic;
using Neptune.NsPay.Caching.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Maintenance
{
    public class MaintenanceViewModel
    {
        public IReadOnlyList<CacheDto> Caches { get; set; }
        
        public bool CanClearAllCaches { get; set; }
    }
}
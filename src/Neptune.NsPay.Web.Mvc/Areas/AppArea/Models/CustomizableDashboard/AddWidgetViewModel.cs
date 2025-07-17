using System.Collections.Generic;
using Neptune.NsPay.DashboardCustomization.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.CustomizableDashboard
{
    public class AddWidgetViewModel
    {
        public List<WidgetOutput> Widgets { get; set; }

        public string DashboardName { get; set; }

        public string PageId { get; set; }
    }
}

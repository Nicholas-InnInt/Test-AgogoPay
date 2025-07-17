using System.Collections.Generic;
using Neptune.NsPay.Editions.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Tenants
{
    public class TenantIndexViewModel
    {
        public List<SubscribableEditionComboboxItemDto> EditionItems { get; set; }
    }
}
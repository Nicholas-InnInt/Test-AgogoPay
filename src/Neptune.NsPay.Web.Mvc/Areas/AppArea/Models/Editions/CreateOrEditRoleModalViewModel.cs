using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Neptune.NsPay.Editions.Dto;
using Neptune.NsPay.Web.Areas.AppArea.Models.Common;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Editions
{
    [AutoMapFrom(typeof(GetEditionEditOutput))]
    public class CreateEditionModalViewModel : GetEditionEditOutput, IFeatureEditViewModel
    {
        public IReadOnlyList<ComboboxItemDto> EditionItems { get; set; }

        public IReadOnlyList<ComboboxItemDto> FreeEditionItems { get; set; }
    }
}
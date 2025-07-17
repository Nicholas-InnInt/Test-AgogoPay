using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Editions.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Common
{
    public interface IFeatureEditViewModel
    {
        List<NameValueDto> FeatureValues { get; set; }

        List<FlatFeatureDto> Features { get; set; }
    }
}
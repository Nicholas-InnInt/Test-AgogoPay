using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Neptune.NsPay.Editions.Dto;

namespace Neptune.NsPay.MultiTenancy.Dto
{
    public class GetTenantFeaturesEditOutput
    {
        public List<NameValueDto> FeatureValues { get; set; }

        public List<FlatFeatureDto> Features { get; set; }
    }
}
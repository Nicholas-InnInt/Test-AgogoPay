using System;
using Abp.Application.Services.Dto;

namespace Neptune.NsPay.Merchants.Dtos
{
    public class MerchantDto : EntityDto
    {
        public string Name { get; set; }

        public string Mail { get; set; }

        public string Phone { get; set; }

        public string MerchantCode { get; set; }

        public string MerchantSecret { get; set; }

        public string PlatformCode { get; set; }

        public int PayGroupId { get; set; }

        public string PayGroupName { get; set; }

        public string CountryType { get; set; }

        public string Remark { get; set; }

    }
}
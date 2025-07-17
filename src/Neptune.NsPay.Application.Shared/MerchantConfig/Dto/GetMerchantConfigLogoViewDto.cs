using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.NsPay.MerchantConfig.Dto
{
    public class GetMerchantConfigLogoViewDto
    {
        public string MerchantCode { get; set; }
        public int MerchantId { get; set; }
        public Guid? LogoId { get; set; }
        public string LogoFileType { get; set; }
    }
}

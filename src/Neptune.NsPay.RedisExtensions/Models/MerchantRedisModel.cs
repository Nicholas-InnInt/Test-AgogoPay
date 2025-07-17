using Neptune.NsPay.Merchants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.RedisExtensions.Models
{
    public class MerchantRedisModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mail { get; set; }
        public string Phone { get; set; }
        public string MerchantCode { get; set; }
        public string MerchantSecret { get; set; }
        public string PlatformCode { get; set; }
        public int PayGroupId { get; set; }
        public MerchantTypeEnum MerchantType { get; set; }
        public MerchantRateRedisModel MerchantRate { get; set; }
        public MerchantSettingRedisModel MerchantSetting { get; set; }
    }
}

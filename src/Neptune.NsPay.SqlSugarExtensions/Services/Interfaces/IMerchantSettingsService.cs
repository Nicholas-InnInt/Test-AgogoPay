using Neptune.NsPay.MerchantSettings;
using Neptune.NsPay.SqlSugarExtensions.DbContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.SqlSugarExtensions.Services
{
    public interface IMerchantSettingsService : IBaseService<MerchantSetting>
    {
    }
}

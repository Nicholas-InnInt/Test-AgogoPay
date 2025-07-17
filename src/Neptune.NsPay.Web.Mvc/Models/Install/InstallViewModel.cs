using System.Collections.Generic;
using Abp.Localization;
using Neptune.NsPay.Install.Dto;

namespace Neptune.NsPay.Web.Models.Install
{
    public class InstallViewModel
    {
        public List<ApplicationLanguage> Languages { get; set; }

        public AppSettingsJsonDto AppSettingsJson { get; set; }
    }
}

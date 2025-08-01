﻿using System.Collections.Generic;

namespace Neptune.NsPay.Web.Models.Consent
{
    public class ConsentViewModel : ConsentInputModel
    {
        public string ClientName { get; set; }

        public string ClientUrl { get; set; }

        public string ClientLogoUrl { get; set; }

        public bool AllowRememberConsent { get; set; }

        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }

        public IEnumerable<ScopeViewModel> ApiScopes { get; set; }
    }
}

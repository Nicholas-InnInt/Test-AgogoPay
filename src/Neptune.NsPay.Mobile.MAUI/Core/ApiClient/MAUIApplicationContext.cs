﻿using Neptune.NsPay.ApiClient;

namespace Neptune.NsPay.Mobile.MAUI.Core.ApiClient
{
    public class MAUIApplicationContext : ApplicationContext
    {
        private TenantInformation _currentTenant;
        public override TenantInformation CurrentTenant
        {
            get => _currentTenant;
            protected set
            {
                if (value == _currentTenant)
                {
                    return;
                }
                _currentTenant = value;
                OnTenantChange?.Invoke(this, EventArgs.Empty);
            }
        }

        public static event EventHandler OnTenantChange;
    }
}

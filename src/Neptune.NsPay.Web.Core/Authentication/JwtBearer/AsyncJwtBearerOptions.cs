using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Neptune.NsPay.Web.Authentication.JwtBearer
{
    public class AsyncJwtBearerOptions : JwtBearerOptions
    {
        public readonly List<IAsyncSecurityTokenValidator> AsyncSecurityTokenValidators;
        
        private readonly NsPayAsyncJwtSecurityTokenHandler _defaultAsyncHandler = new NsPayAsyncJwtSecurityTokenHandler();

        public AsyncJwtBearerOptions()
        {
            AsyncSecurityTokenValidators = new List<IAsyncSecurityTokenValidator>() {_defaultAsyncHandler};
        }
    }

}

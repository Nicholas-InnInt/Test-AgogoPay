using Microsoft.EntityFrameworkCore;
using Neptune.NsPay.OpenIddict.Applications;
using Neptune.NsPay.OpenIddict.Authorizations;
using Neptune.NsPay.OpenIddict.Scopes;
using Neptune.NsPay.OpenIddict.Tokens;

namespace Neptune.NsPay.EntityFrameworkCore
{
    public interface IOpenIddictDbContext
    {
        DbSet<OpenIddictApplication> Applications { get; }

        DbSet<OpenIddictAuthorization> Authorizations { get; }

        DbSet<OpenIddictScope> Scopes { get; }

        DbSet<OpenIddictToken> Tokens { get; }
    }

}
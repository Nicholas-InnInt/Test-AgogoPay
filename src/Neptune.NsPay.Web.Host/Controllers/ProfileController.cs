using Abp.AspNetCore.Mvc.Authorization;
using Neptune.NsPay.Authorization.Users.Profile;
using Neptune.NsPay.Graphics;
using Neptune.NsPay.Storage;

namespace Neptune.NsPay.Web.Controllers
{
    [AbpMvcAuthorize]
    public class ProfileController : ProfileControllerBase
    {
        public ProfileController(
            ITempFileCacheManager tempFileCacheManager,
            IProfileAppService profileAppService,
            IImageValidator imageValidator) :
            base(tempFileCacheManager, profileAppService, imageValidator)
        {
        }
    }
}
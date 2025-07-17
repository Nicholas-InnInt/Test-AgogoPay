using Abp.AutoMapper;
using Neptune.NsPay.Authorization.Roles.Dto;
using Neptune.NsPay.Web.Areas.AppArea.Models.Common;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.Roles
{
    [AutoMapFrom(typeof(GetRoleForEditOutput))]
    public class CreateOrEditRoleModalViewModel : GetRoleForEditOutput, IPermissionsEditViewModel
    {
        public bool IsEditMode => Role.Id.HasValue;
    }
}
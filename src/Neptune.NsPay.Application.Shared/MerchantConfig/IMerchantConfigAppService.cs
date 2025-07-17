using Abp.Application.Services;
using Neptune.NsPay.Authorization.Users.Profile.Dto;
using Neptune.NsPay.MerchantConfig.Dto;
using Neptune.NsPay.MerchantSettings.Dtos;
using Neptune.NsPay.MerchantWithdrawBanks.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.MerchantConfig
{
	public interface IMerchantConfigAppService: IApplicationService
	{
        Task<GetMerchantConfigViewDto> GetMerchantConfig();

        Task<GetMerchantConfigLogoViewDto> GetMerchantUserAsync(Guid logoId);

        Task<GetProfilePictureOutput> GetMerchantLogoPicture();

        Task UpdateMerchantConfigTitle(GetMerchantConfigInformationInput input);

        Task UpdateMerchantConfigIpAddress(GetMerchantConfigIpAddressInput input);

        Task UpdateMerchantNotify(GetMerchantConfigNotifyInput input);

        Task UpdateMerchantPlatFromWithdraw(GetMerchantPlatFromWithdrawInput input);

        Task<List<MerchantWithdrawBankDto>> GetMerchantBanks();

    }
}

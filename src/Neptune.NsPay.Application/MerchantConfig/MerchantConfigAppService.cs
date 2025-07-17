using Abp.AspNetZeroCore.Net;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Authorization.Users.Profile.Dto;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantConfig.Dto;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MerchantSettings;
using Neptune.NsPay.MerchantSettings.Dtos;
using Neptune.NsPay.MerchantWithdrawBanks;
using Neptune.NsPay.MerchantWithdrawBanks.Dtos;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.Storage;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.MerchantConfig
{
	[AbpAuthorize(AppPermissions.Pages_MerchantConfig)]
	public class MerchantConfigAppService: NsPayAppServiceBase, IMerchantConfigAppService
	{
		private readonly IRepository<MerchantSetting> _merchantSettingRepository;
        private readonly IRepository<MerchantWithdrawBank> _merchantWithdrawBankRepository;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IRedisService _redisService;
        public MerchantConfigAppService(IBinaryObjectManager binaryObjectManager,
            IRepository<MerchantSetting> merchantSettingRepository,
            IRepository<MerchantWithdrawBank> merchantWithdrawBankRepository,
            IRedisService redisService)
        {
            _binaryObjectManager = binaryObjectManager;
            _merchantSettingRepository = merchantSettingRepository;
            _merchantWithdrawBankRepository = merchantWithdrawBankRepository;
            _redisService = redisService;
        }


        public async Task<GetMerchantConfigViewDto> GetMerchantConfig()
        {
            GetMerchantConfigViewDto resultDto = new GetMerchantConfigViewDto();
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants;
            if (user.UserType == UserTypeEnum.InternalMerchant)
            {
                if (merchants.Count == 1)
                {
                    var merchant = merchants[0];
                    var merchantSetting = await _merchantSettingRepository.FirstOrDefaultAsync(r => r.MerchantId == merchant.Id);
                    List<MerchantConfigBank> banks = new List<MerchantConfigBank>();
                    if (merchantSetting != null)
                    {
                        resultDto.MerchantCode = merchantSetting.MerchantCode;
                        resultDto.MerchantId = merchantSetting.MerchantId;
                        resultDto.Title = merchantSetting.NsPayTitle;
                        resultDto.OrderBankRemark = merchantSetting.OrderBankRemark;
                        resultDto.LogoUrl = merchantSetting.LogoUrl;
                        resultDto.LoginIpAddress = merchantSetting.LoginIpAddress;
                        resultDto.BankNotifyText = merchantSetting.BankNotifyText;
                        resultDto.TelegramNotifyBotId = merchantSetting.TelegramNotifyBotId;
                        resultDto.TelegramNotifyChatId = merchantSetting.TelegramNotifyChatId;
                        resultDto.OpenRiskWithdrawal = merchantSetting.OpenRiskWithdrawal;
                        resultDto.PlatformUrl = merchantSetting.PlatformUrl;
                        resultDto.PlatformUserName = merchantSetting.PlatformUserName;
                        resultDto.PlatformPassWord = merchantSetting.PlatformPassWord;
                        resultDto.PlatformLimitMoney = merchantSetting.PlatformLimitMoney;

                        if (!merchantSetting.BankNotify.IsNullOrEmpty())
                        {
                            var tempBanks = merchantSetting.BankNotify.FromJsonString<List<MerchantConfigBank>>();
                            resultDto.MerchantConfigBank = tempBanks;
                        }
                        else
                        {
                            resultDto.MerchantConfigBank = banks;
                        }
                    }
                    else
                    {
                        resultDto.MerchantCode = merchant.MerchantCode;
                        resultDto.MerchantId = merchant.Id;
                        resultDto.Title = "NsPay";
                        resultDto.Title = "";
                        resultDto.LogoUrl = "";
                        resultDto.LoginIpAddress = "";
                        resultDto.BankNotifyText = "";
                        resultDto.TelegramNotifyBotId = "";
                        resultDto.TelegramNotifyChatId = "";
                        resultDto.TelegramNotifyChatId = "";
                        resultDto.MerchantConfigBank = banks;
                        resultDto.OpenRiskWithdrawal = false;
                        resultDto.PlatformUrl = "";
                        resultDto.PlatformUserName = "";
                        resultDto.PlatformPassWord = "";
                        resultDto.PlatformLimitMoney = 0;
                    }
                }
            }
            return resultDto;
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantConfig_Create)]
        public async Task<GetMerchantConfigLogoViewDto> GetMerchantUserAsync(Guid logoId)
        {
            var user = await GetCurrentUserAsync();

            var merchants = user.Merchants;
            if (user.UserType == UserTypeEnum.InternalMerchant)
            {
                if (merchants.Count == 1)
                {
                    var merchant = merchants[0];

                    var config = await _merchantSettingRepository.FirstOrDefaultAsync(r => r.MerchantCode == merchant.MerchantCode);
                    if (config == null)
                    {
                        //保存数据
                        MerchantSetting merchantSetting = new MerchantSetting()
                        {
                            MerchantCode = merchant.MerchantCode,
                            MerchantId = merchant.Id,
                            LogoUrl = logoId.ToString(),
                        };
                        await _merchantSettingRepository.InsertAsync(merchantSetting);
                    }
                    else
                    {
                        config.MerchantId = merchant.Id;
                        config.LogoUrl = logoId.ToString();
                        await _merchantSettingRepository.UpdateAsync(config);
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                    _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantSetting + merchant.MerchantCode, config);

                    GetMerchantConfigLogoViewDto viewDto = new GetMerchantConfigLogoViewDto()
                    {
                        MerchantCode = merchant.MerchantCode,
                        MerchantId = merchant.Id,
                    };
                    return viewDto;
                }
            }

            return null;
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantConfig_Create)]
        public async Task UpdateMerchantConfigTitle(GetMerchantConfigInformationInput input)
        {
            var merchantSetting = await _merchantSettingRepository.FirstOrDefaultAsync(r => r.MerchantCode == input.MerchantCode);

            if (merchantSetting == null)
            {
                merchantSetting = new MerchantSetting()
                {
                    MerchantCode = input.MerchantCode,
                    MerchantId= input.MerchantId,
                    NsPayTitle = input.Title,
                    OrderBankRemark = input.OrderBankRemark,
                };
                await _merchantSettingRepository.InsertAsync(merchantSetting);
            }
            else
            {
                merchantSetting.NsPayTitle = input.Title;
                merchantSetting.OrderBankRemark = input.OrderBankRemark;
                await _merchantSettingRepository.UpdateAsync(merchantSetting);
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantSetting + input.MerchantCode, merchantSetting);
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantConfig_Create)]
        public async Task UpdateMerchantConfigIpAddress(GetMerchantConfigIpAddressInput input)
        {
            var merchantSetting = await _merchantSettingRepository.FirstOrDefaultAsync(r => r.MerchantCode == input.MerchantCode);

            if (merchantSetting == null)
            {
                merchantSetting = new MerchantSetting()
                {
                    MerchantCode = input.MerchantCode,
                    MerchantId = input.MerchantId,
                    LoginIpAddress = input.LoginIpAddress
                };
                await _merchantSettingRepository.InsertAsync(merchantSetting);
            }
            else
            {
                merchantSetting.LoginIpAddress = input.LoginIpAddress;
                await _merchantSettingRepository.UpdateAsync(merchantSetting);
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantSetting + input.MerchantCode, merchantSetting);
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantConfig_Create)]
        public async Task UpdateMerchantNotify(GetMerchantConfigNotifyInput input)
        {
            var merchantSetting = await _merchantSettingRepository.FirstOrDefaultAsync(r => r.MerchantCode == input.MerchantCode);

            if (merchantSetting == null)
            {
                merchantSetting = new MerchantSetting()
                {
                    MerchantCode = input.MerchantCode,
                    BankNotifyText = input.BankNotifyText,
                    TelegramNotifyBotId = input.TelegramNotifyBotId,
                    TelegramNotifyChatId = input.TelegramNotifyChatId,
                    BankNotify = input.MerchantConfigBank.ToJsonString()
                };
                await _merchantSettingRepository.InsertAsync(merchantSetting);
            }
            else
            {
                if (input.MerchantConfigBank != null && input.MerchantConfigBank.Count > 0)
                {
                    merchantSetting.BankNotify = input.MerchantConfigBank.ToJsonString();
                }
                merchantSetting.BankNotifyText = input.BankNotifyText;
                merchantSetting.TelegramNotifyBotId = input.TelegramNotifyBotId;
                merchantSetting.TelegramNotifyChatId = input.TelegramNotifyChatId;
                await _merchantSettingRepository.UpdateAsync(merchantSetting);
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantSetting + input.MerchantCode, merchantSetting);
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantConfig_Create)]
        public async Task UpdateMerchantPlatFromWithdraw(GetMerchantPlatFromWithdrawInput input)
        {
            var merchantSetting = await _merchantSettingRepository.FirstOrDefaultAsync(r => r.MerchantCode == input.MerchantCode);

            if (merchantSetting == null)
            {
                merchantSetting = new MerchantSetting()
                {
                    MerchantCode = input.MerchantCode,
                    MerchantId = input.MerchantId,
                    OpenRiskWithdrawal = input.OpenRiskWithdrawal,
                    PlatformUrl = input.PlatformUrl,
                    PlatformUserName = input.PlatformUserName,
                    PlatformPassWord = input.PlatformPassWord,
                    PlatformLimitMoney = input.PlatformLimitMoney
                };
                await _merchantSettingRepository.InsertAsync(merchantSetting);
            }
            else
            {
                merchantSetting.OpenRiskWithdrawal = input.OpenRiskWithdrawal;
                merchantSetting.PlatformUrl = input.PlatformUrl;
                merchantSetting.PlatformUserName = input.PlatformUserName;
                merchantSetting.PlatformPassWord = input.PlatformPassWord;
                merchantSetting.PlatformLimitMoney = input.PlatformLimitMoney;
                await _merchantSettingRepository.UpdateAsync(merchantSetting);
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            _redisService.AddRedisValue(NsPayRedisKeyConst.MerchantSetting + input.MerchantCode, merchantSetting);
        }

        [AbpAuthorize(AppPermissions.Pages_MerchantConfig_Create)]
        public async Task<GetProfilePictureOutput> GetMerchantLogoPicture()
        {
            var user = await GetCurrentUserAsync();

            var merchants = user.Merchants;

            if (merchants.Count == 1)
            {
                var merchant = merchants[0];
                var config = await _merchantSettingRepository.FirstOrDefaultAsync(r => r.MerchantCode == merchant.MerchantCode && r.MerchantId == merchant.Id);
                if (config != null)
                {
                    var file = await _binaryObjectManager.GetOrNullAsync(Guid.Parse(config.LogoUrl));
                    var profilePictureContent = file == null ? "" : Convert.ToBase64String(file.Bytes);

                    return new GetProfilePictureOutput(profilePictureContent);
                }
            }
            return new GetProfilePictureOutput("");
        }

        //[AbpAuthorize(AppPermissions.Pages_MerchantConfig_Create)]
        //public async Task changeTcbBankIp()
        //{
        //    var user = await GetCurrentUserAsync();
        //    var merchants = user.Merchants;
        //    if (merchants.Count == 1)
        //    {
        //        var urlIp = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.SocketChangIpApi);
        //        var bankIdStr = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.SocketTcbPort);
        //        if(!urlIp.IsNullOrEmpty() && !bankIdStr.IsNullOrEmpty())
        //        {
        //            var ports = bankIdStr.FromJsonString<List<NameValueRedisModel>>();
        //            if (ports != null)
        //            {
        //                var port = ports.FirstOrDefault(r => r.Name == merchants.FirstOrDefault().MerchantCode);
        //                if (port != null)
        //                {
        //                    var url = urlIp + "cip?port=" + port.Value;
        //                    var client = new RestClient(url);
        //                    var request = new RestRequest();
        //                    var response = await client.ExecuteAsync(request);
        //                }
        //            }
        //        }
        //    }
        //}

        //[AbpAuthorize(AppPermissions.Pages_MerchantConfig_Create)]
        //public async Task changeVcbBankIp()
        //{
        //    var user = await GetCurrentUserAsync();
        //    var merchants = user.Merchants;
        //    if (merchants.Count == 1)
        //    {
        //        var urlIp = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.SocketChangIpApi);
        //        var bankIdStr = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.SocketVcbPort);
        //        if (!urlIp.IsNullOrEmpty() && !bankIdStr.IsNullOrEmpty())
        //        {
        //            var ports = bankIdStr.FromJsonString<List<NameValueRedisModel>>();
        //            if (ports != null)
        //            {
        //                var port = ports.FirstOrDefault(r => r.Name == merchants.FirstOrDefault().MerchantCode);
        //                if (port != null)
        //                {
        //                    var url = urlIp + "cip?port=" + port.Value;
        //                    var client = new RestClient(url);
        //                    var request = new RestRequest();
        //                    await client.ExecuteAsync(request);
        //                }
        //            }
        //        }
        //    }
        //}

        [AbpAuthorize(AppPermissions.Pages_MerchantConfig_Create)]
        public async Task<List<MerchantWithdrawBankDto>> GetMerchantBanks()
        {
            List<MerchantWithdrawBankDto> withdrawBankDtos = new List<MerchantWithdrawBankDto>();
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants;
            if (merchants.Count == 1)
            {
                var merchantCode = merchants[0].MerchantCode;
                var banks = await _merchantWithdrawBankRepository.GetAllListAsync(r => r.MerchantCode == merchantCode && r.Status == true);
                foreach (var bank in banks)
                {
                    var info = ObjectMapper.Map<MerchantWithdrawBankDto>(bank);
                    withdrawBankDtos.Add(info);
                }
            }
            return withdrawBankDtos;
        }

    }
}

using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Common;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.NsPayBackgroundJobs;
using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.RedisExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.PayMents
{
    [AbpAuthorize(AppPermissions.Pages_PayMents)]
    public class PayMentsAppService : NsPayAppServiceBase, IPayMentsAppService
    {
        private static readonly string Code = "NsPay";
        private readonly IRepository<PayMent> _payMentRepository;
        private readonly IRepository<PayGroupMent> _payGroupMentRepository;
        private readonly ICommonPayGroupMentRedisAppService _commonPayGroupMentRedisAppService;
        private readonly IRepository<NsPayBackgroundJob, Guid> _nsPayBackgroundJobRepository;
        private readonly IRepository<PayGroup> _payGroupRepository;
        private readonly IRedisService _redisService;
        private readonly IConfigurationRoot _appConfiguration;

        public PayMentsAppService(
            IRepository<PayMent> payMentRepository,
            IRepository<PayGroupMent> payGroupMentRepository,
            ICommonPayGroupMentRedisAppService commonPayGroupMentRedisAppService,
            IRepository<NsPayBackgroundJob, Guid> nsPayBackgroundJobRepository,
            IRepository<PayGroup> payGroupRepository,
            IRedisService redisService,
            IAppConfigurationAccessor appConfigurationAccessor)
        {
            _payMentRepository = payMentRepository;
            _payGroupMentRepository = payGroupMentRepository;
            _commonPayGroupMentRedisAppService = commonPayGroupMentRedisAppService;
            _nsPayBackgroundJobRepository = nsPayBackgroundJobRepository;
            _payGroupRepository = payGroupRepository;
            _redisService = redisService;
            _appConfiguration = appConfigurationAccessor.Configuration;
        }

        public virtual async Task<PagedResultDto<GetPayMentForViewDto>> GetAll(GetAllPayMentsInput input)
        {
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants;

            var typeFilter = input.TypeFilter.HasValue
                        ? (PayMentTypeEnum)input.TypeFilter
                        : default;
            var companyTypeFilter = input.CompanyTypeFilter.HasValue
                ? (PayMentCompanyTypeEnum)input.CompanyTypeFilter
                : default;
            PayMentStatusEnum? showStatusFilter = null;
            if (input.ShowStatusFilter.HasValue && input.ShowStatusFilter > -1)
            {
                showStatusFilter = (PayMentStatusEnum)input.ShowStatusFilter;
            }
            bool useMoMoFilter = false;
            if (input.UseMoMoFilter.HasValue && input.UseMoMoFilter > -1)
            {
                useMoMoFilter = Convert.ToBoolean(input.UseMoMoFilter);
            }
            bool payMentStatusFilter = false;
            if (input.PayMentStatusFilter.HasValue && input.PayMentStatusFilter > -1)
            {
                payMentStatusFilter = Convert.ToBoolean(input.PayMentStatusFilter);
            }

            var filteredPayMents = _payMentRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.Name.Contains(input.Filter) || e.FullName.Contains(input.Filter) || e.Phone.Contains(input.Filter) || e.CardNumber.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.NameFilter), e => e.Name.Contains(input.NameFilter))
                        .WhereIf(input.TypeFilter.HasValue && input.TypeFilter > -1, e => e.Type == typeFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PhoneFilter), e => e.Phone.Contains(input.PhoneFilter))
                        .WhereIf(input.ShowStatusFilter.HasValue && input.ShowStatusFilter > -1, e => e.Status == showStatusFilter)
                        .WhereIf(input.UseMoMoFilter.HasValue && input.UseMoMoFilter > -1, e => e.UseMoMo == useMoMoFilter);

            if (user.Surname.ToUpper() != "ADMIN")
            {
                var payGroups = user.Merchants.Select(r => r.PayGroupId);
                List<int> payments = new List<int>();
                if (payMentStatusFilter)
                {
                    payments = _payGroupMentRepository.GetAll().Where(r => payGroups.Contains(r.GroupId) && r.Status == true && r.IsDeleted == false).Select(r => r.PayMentId).ToList();
                }
                else
                {
                    if (input.PayMentStatusFilter.HasValue && input.PayMentStatusFilter > -1)
                    {
                        payments = _payGroupMentRepository.GetAll().Where(r => payGroups.Contains(r.GroupId) && r.Status == false && r.IsDeleted == false).Select(r => r.PayMentId).ToList();
                    }
                    else
                    {
                        payments = _payGroupMentRepository.GetAll().Where(r => payGroups.Contains(r.GroupId) && r.IsDeleted == false).Select(r => r.PayMentId).ToList();
                    }
                }
                filteredPayMents = filteredPayMents.Where(r => payments.Contains(r.Id));
            }

            var pagedAndFilteredPayMents = filteredPayMents
                .OrderBy(input.Sorting ?? "id desc")
                .PageBy(input);

            var payMents = from o in pagedAndFilteredPayMents
                           select new
                           {
                               o.Name,
                               o.Type,
                               o.CompanyType,
                               o.Phone,
                               o.Mail,
                               o.Status,
                               o.Id,
                               o.CardNumber,
                               o.FullName,
                               o.UseMoMo,
                               o.MinMoney,
                               o.MaxMoney,
                               o.LimitMoney,
                               o.BalanceLimitMoney,
                               o.MoMoRate,
                               o.ZaloRate,
                               o.VittelPayRate,
                               o.CryptoWalletAddress,
                           };

            var totalCount = await filteredPayMents.CountAsync();

            var dbList = await payMents.ToListAsync();
            var results = new List<GetPayMentForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetPayMentForViewDto()
                {
                    PayMent = new PayMentDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Type = o.Type,
                        CompanyType = o.CompanyType,
                        Phone = o.Phone,
                        CardNumber = o.CardNumber,
                        FullName = o.FullName,
                        Status = o.Status,
                        UseMoMo = o.UseMoMo,
                        MinMoney = o.MinMoney,
                        MaxMoney = o.MaxMoney,
                        LimitMoney = o.LimitMoney,
                        BalanceLimitMoney = o.BalanceLimitMoney,
                        CryptoWalletAddress = o.CryptoWalletAddress,
                        PayMentStatus = GetPayGroupMentStatus(o.Id),
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetPayMentForViewDto>(
                totalCount,
                results
            );
        }

        public virtual async Task<GetPayMentForViewDto> GetPayMentForView(int id)
        {
            var payMent = await _payMentRepository.GetAsync(id);

            var output = new GetPayMentForViewDto { PayMent = ObjectMapper.Map<PayMentDto>(payMent) };

            return output;
        }

        //[AbpAuthorize(AppPermissions.Pages_PayMents_Edit)]
        public virtual async Task<GetPayMentForEditOutput> GetPayMentForEdit(EntityDto input)
        {
            var payMent = await _payMentRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetPayMentForEditOutput { PayMent = ObjectMapper.Map<CreateOrEditPayMentDto>(payMent) };

            return output;
        }

        public virtual async Task CreateOrEdit(CreateOrEditPayMentDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayMents_Create)]
        protected virtual async Task Create(CreateOrEditPayMentDto input)
        {
            var user = await GetCurrentUserAsync();
            var typeList = new List<PayMentTypeEnum>() {
                PayMentTypeEnum.ScratchCards,
                PayMentTypeEnum.MoMoPay,
                PayMentTypeEnum.ZaloPay,
                PayMentTypeEnum.ViettelPay
            };

            if (!typeList.Contains(input.Type))
            {
                input.CompanyType = 0;
            }
            input.Phone = input.Phone.Trim(); //front end is LoginAccount
            input.Name = input.Name.Trim();

            if (user.UserType == UserTypeEnum.InternalMerchant)
            {
                if (user.Merchants.FirstOrDefault() is { PayGroupId: > 0 } merchant)
                {
                    var payMent = ObjectMapper.Map<PayMent>(input);
                    var payMentId = await _payMentRepository.InsertAndGetIdAsync(payMent);

                    var payGroupMent = new PayGroupMent
                    {
                        GroupId = merchant.PayGroupId,
                        PayMentId = payMentId,
                        Status = false,
                        CreationTime = DateTime.Now,
                    };
                    await _payGroupMentRepository.InsertAsync(payGroupMent);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    //更新缓存
                    await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValue(merchant.PayGroupId);
                    var psyMentDto = ObjectMapper.Map<PayMentDto>(payMent);
                    _commonPayGroupMentRedisAppService.AddPayMentRedisValue(psyMentDto);
                }
            }

            if (user.UserType == UserTypeEnum.NsPayAdmin)
            {
                var payMent = ObjectMapper.Map<PayMent>(input);
                await _payMentRepository.InsertAsync(payMent);

                await CurrentUnitOfWork.SaveChangesAsync();

                var psyMentDto = ObjectMapper.Map<PayMentDto>(payMent);
                _commonPayGroupMentRedisAppService.AddPayMentRedisValue(psyMentDto);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayMents_Edit)]
        protected virtual async Task Update(CreateOrEditPayMentDto input)
        {
            var payMent = await _payMentRepository.FirstOrDefaultAsync((int)input.Id);
            var typeList = new List<PayMentTypeEnum>() {
                PayMentTypeEnum.ScratchCards,
                PayMentTypeEnum.MoMoPay,
                PayMentTypeEnum.ZaloPay,
                PayMentTypeEnum.ViettelPay
            };

            if (!typeList.Contains(input.Type))
            {
                input.CompanyType = 0;
            }
            input.Phone = input.Phone.Trim(); //front end is LoginAccount
            input.Name = input.Name.Trim();
            ObjectMapper.Map(input, payMent);

            await CurrentUnitOfWork.SaveChangesAsync();

            //更新缓存
            await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValueByPayMentId((int)input.Id);

            var psyMentDto = ObjectMapper.Map<PayMentDto>(payMent);
            _commonPayGroupMentRedisAppService.AddPayMentRedisValue(psyMentDto);
        }

        [AbpAuthorize(AppPermissions.Pages_PayMents_Delete)]
        public virtual async Task Delete(EntityDto input)
        {
            await _payMentRepository.DeleteAsync(input.Id);

            await CurrentUnitOfWork.SaveChangesAsync();

            //更新缓存
            await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValueByPayMentId((int)input.Id);

            _redisService.DeletePayMentCaches(input.Id.ToString());
        }

        [AbpAuthorize(AppPermissions.Pages_PayMents_OpenJob)]
        public virtual async Task OpenJob(EntityDto input)
        {
            var openJobSupportedBank = new List<PayMentTypeEnum>() { PayMentTypeEnum.ACBBank, PayMentTypeEnum.MBBank };
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants;
            var payMent = await _payMentRepository.FirstOrDefaultAsync((int)input.Id);
            if (payMent != null && openJobSupportedBank.Contains(payMent.Type)) // check supported bank
            {
                var BankApi = "";
                var MerchantCode = "";
                if (user.UserType == UserTypeEnum.InternalMerchant)
                {
                    var merchant = merchants[0];
                    if (merchant.PayGroupId > 0)
                    {
                        //获取缓存
                        var payGroup = await _payGroupRepository.FirstOrDefaultAsync(r => r.Id == merchant.PayGroupId);
                        if (payMent.Type == PayMentTypeEnum.VietcomBank)
                        {
                            BankApi = payGroup?.VietcomApi;
                        }
                        else
                        {
                            BankApi = payGroup?.BankApi;
                        }
                        if (payMent.BusinessType == true && payMent.Type == PayMentTypeEnum.ACBBank)
                        {
                            BankApi = "http://bank2.nsmanage.top/";
                        }
                        MerchantCode = merchant.MerchantCode;
                    }
                }
                if (user.UserType == UserTypeEnum.NsPayAdmin)
                {
                    if (payMent.Type == PayMentTypeEnum.VietcomBank)
                    {
                        BankApi = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.VcbBankApi);
                    }
                    else
                    {
                        BankApi = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.BankApi);
                        if (payMent.BusinessType == true && payMent.Type == PayMentTypeEnum.ACBBank)
                        {
                            BankApi = "http://bank2.nsmanage.top/";
                        }
                    }
                    MerchantCode = Code;
                }

                if (!string.IsNullOrEmpty(BankApi))
                {
                    var name = payMent.Type.ToString() + "-" + payMent.Phone.Trim() + "-" + payMent.CardNumber;
                    var jobs = await _nsPayBackgroundJobRepository.FirstOrDefaultAsync(r => r.Name == name);
                    if (jobs == null)
                    {
                        Random rnd = new Random(Guid.NewGuid().GetHashCode());
                        var time = rnd.Next(15, 30);
                        var api = GetApi(payMent.Type, payMent.DispenseType, MerchantCode);
                        if (!string.IsNullOrEmpty(api))
                        {
                            //添加定时任务，禁用状态
                            NsPayBackgroundJob nsPayBackgroundJob = new NsPayBackgroundJob()
                            {
                                Name = name,
                                GroupName = "BankJob",
                                ApiUrl = GetApi(payMent.Type, payMent.DispenseType, MerchantCode),
                                Cron = "0/" + time + " * * * * ?",
                                Description = name,
                                State = NsPayBackgroundJobStateEnum.Pending,
                                RequsetMode = NsPayBackgroundJobRequsetModeEnum.Post,
                                ParamData = "{\"Phone\":\"" + payMent.Phone + "\",\"BankApi\":\"" + BankApi + "\",\"CardNumber\":\"" + payMent.CardNumber + "\"}",
                                MerchantCode = MerchantCode
                            };
                            await _nsPayBackgroundJobRepository.InsertAsync(nsPayBackgroundJob);
                        }
                    }
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayMents_EditStatus)]
        public virtual async Task Hide(EntityDto input)
        {
            var payMent = await _payMentRepository.FirstOrDefaultAsync((int)input.Id);
            if (payMent != null)
            {
                if (payMent.Status == PayMentStatusEnum.Show)
                {
                    payMent.Status = PayMentStatusEnum.Hide;
                }
                else
                {
                    payMent.Status = PayMentStatusEnum.Show;
                }
                await _payMentRepository.UpdateAsync(payMent);

                await CurrentUnitOfWork.SaveChangesAsync();
                //更新缓存
                await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValueByPayMentId((int)input.Id);

                var psyMentDto = ObjectMapper.Map<PayMentDto>(payMent);
                _commonPayGroupMentRedisAppService.AddPayMentRedisValue(psyMentDto);
            }
        }

        public async Task EditPayMent(CreateOrEditPayMentDto input)
        {
            if (input.Id != null)
            {
                await UpdatePayMent(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayMents_ChildEdit)]
        protected virtual async Task UpdatePayMent(CreateOrEditPayMentDto input)
        {
            var payMent = await _payMentRepository.FirstOrDefaultAsync((int)input.Id);
            if (payMent != null)
            {
                payMent.BalanceLimitMoney = input.BalanceLimitMoney;
                payMent.LimitMoney = input.LimitMoney;
                payMent.MinMoney = input.MinMoney;
                payMent.MaxMoney = input.MaxMoney;
                await _payMentRepository.UpdateAsync(payMent);

                await CurrentUnitOfWork.SaveChangesAsync();
                //更新缓存
                await _commonPayGroupMentRedisAppService.AddPayGroupMentRedisValueByPayMentId((int)input.Id);

                var psyMentDto = ObjectMapper.Map<PayMentDto>(payMent);
                _commonPayGroupMentRedisAppService.AddPayMentRedisValue(psyMentDto);
            }
        }

        protected string GetApi(PayMentTypeEnum type, PayMentDispensEnum isPay = PayMentDispensEnum.None, string merchant = "")
        {
            if (type == PayMentTypeEnum.MBBank)
            {
                return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.MbBankJobApi);
            }
            if (type == PayMentTypeEnum.VietcomBank)
            {
                return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.VcbBankJobApi);
            }
            if (type == PayMentTypeEnum.VietinBank)
            {
                return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.VtbBankJobApi);
            }
            if (type == PayMentTypeEnum.TechcomBank)
            {
                if (isPay == PayMentDispensEnum.None)
                {
                    if (merchant == Code)
                    {
                        return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.TcbBankJobApi);
                    }
                    else
                    {
                        var merchantUrl = _redisService.GetMerchantTcbBankJobApi();
                        if (merchantUrl != null)
                        {
                            var info = merchantUrl.FirstOrDefault(r => r.MerchantCode == merchant);
                            if (info != null)
                            {
                                return info.ApiUrl;
                            }
                            else
                            {
                                return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.TcbBankJobApi);
                            }
                        }
                        else
                        {
                            return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.TcbBankJobApi);
                        }
                    }
                }
                else
                {
                    return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.TcboutbBankJobApi);
                }
            }
            if (type == PayMentTypeEnum.BidvBank)
            {
                return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.BidvBankJobApi);
            }
            if (type == PayMentTypeEnum.ACBBank)
            {
                return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.AcbBankJobApi);
            }
            if (type == PayMentTypeEnum.PVcomBank)
            {
                return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.PvcomBankJobApi);
            }
            if (type == PayMentTypeEnum.BusinessMbBank)
            {
                return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.BusinessMbBankJobApi);
            }
            if (type == PayMentTypeEnum.BusinessVtbBank)
            {
                return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.BusinessVtbBankJobApi);
            }
            if (type == PayMentTypeEnum.BusinessTcbBank)
            {
                return _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.BusinessTcbBankJobApi);
            }
            return "";
        }

        protected bool GetPayGroupMentStatus(int paymentId)
        {
            bool status = false;
            var paygroupMent = _payGroupMentRepository.FirstOrDefault(r => r.PayMentId == paymentId);
            if (paygroupMent != null)
            {
                status = paygroupMent.Status;
            }
            return status;
        }

        [AbpAuthorize(AppPermissions.Pages_PayMents_GetHistory)]
        public virtual async Task GetHistory(EntityDto input)
        {
            try
            {
                var payMent = await _payMentRepository.FirstOrDefaultAsync(input.Id);
                if (payMent != null)
                {
                    var name = payMent.Type.ToString() + "-" + payMent.Phone.Trim() + "-" + payMent.CardNumber;
                    var jobs = await _nsPayBackgroundJobRepository.FirstOrDefaultAsync(r => r.Name == name);
                    if (jobs != null)
                    {
                        if (!jobs.ParamData.IsNullOrEmpty())
                        {
                            var baseUrl = _appConfiguration["BankApiWebSiteUrl"];
                            var bankUrl = string.Empty;
                            if (payMent.Type == PayMentTypeEnum.MBBank)
                            {
                                bankUrl = "MbBank/GetHistory";
                            }
                            else if (payMent.Type == PayMentTypeEnum.BusinessMbBank)
                            {
                                bankUrl = "BusinessMbBank/GetHistory";
                            }
                            else if (payMent.Type == PayMentTypeEnum.VietcomBank)
                            {
                                bankUrl = "VietcomBank/GetHistory";
                            }
                            else if (payMent.Type == PayMentTypeEnum.VietinBank)
                            {
                                bankUrl = "VietinBank/GetHistory";
                            }
                            else if (payMent.Type == PayMentTypeEnum.TechcomBank || payMent.Type == PayMentTypeEnum.BusinessTcbBank)
                            {
                                bankUrl = "TechcomBank/GetHistory";
                            }
                            else if (payMent.Type == PayMentTypeEnum.BidvBank)
                            {
                                bankUrl = "BidvBank/GetHistory";
                            }
                            else if (payMent.Type == PayMentTypeEnum.ACBBank)
                            {
                                bankUrl = "AcbBank/GetHistory";
                            }
                            else if (payMent.Type == PayMentTypeEnum.PVcomBank)
                            {
                                bankUrl = "PVcomBank/GetHistory";
                            }
                            else if (payMent.Type == PayMentTypeEnum.BusinessMbBank)
                            {
                                bankUrl = "BusinessMbBank/GetHistory";
                            }
                            else if (payMent.Type == PayMentTypeEnum.BusinessVtbBank)
                            {
                                bankUrl = "BusinessVtbBank/GetHistory";
                            }

                            if (!baseUrl.IsNullOrEmpty() && !bankUrl.IsNullOrEmpty())
                            {
                                var url = baseUrl + bankUrl;

                                var content = new StringContent(jobs.ParamData, Encoding.UTF8, "application/json");
                                await new HttpClient().PostAsync(url, content);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Warn("支付抓取数据异常：" + ex.ToString());
            }
        }
    }
}
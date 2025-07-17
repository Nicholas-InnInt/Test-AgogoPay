using Abp.Authorization;
using Abp.Domain.Repositories;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune.NsPay.Common
{
    [AbpAuthorize(AppPermissions.Pages_PayMents)]
    public class CommonPayGroupMentRedisAppService : NsPayAppServiceBase, ICommonPayGroupMentRedisAppService
    {
        private readonly IRepository<PayGroup> _payGroupRepository;
        private readonly IRepository<PayMent> _payMentRepository;
        private readonly IRepository<PayGroupMent> _payGroupMentRepository;
        private readonly IRedisService _redisService;

        public CommonPayGroupMentRedisAppService(
            IRepository<PayGroup> payGroupRepository,
            IRepository<PayMent> payMentRepository,
            IRepository<PayGroupMent> payGroupMentRepository,
            IRedisService redisService)
        {
            _payGroupRepository = payGroupRepository;
            _payMentRepository = payMentRepository;
            _payGroupMentRepository = payGroupMentRepository;
            _redisService = redisService;
        }

        [AbpAuthorize(AppPermissions.Pages_PayMents)]
        public async Task AddPayGroupMentRedisValue(int payGroupId)
        {
            var payGroup = await _payGroupRepository.FirstOrDefaultAsync(r => r.Id == payGroupId);

            if (payGroup != null) // handle case when paygroup deleted
            {
                var groupPayments = await _payGroupMentRepository.GetAllListAsync(r => r.GroupId == payGroup.Id && r.IsDeleted == false);
                var groupMentIds = groupPayments.Select(r => r.PayMentId);
                var payMents = await _payMentRepository.GetAllListAsync(r => groupMentIds.Contains(r.Id) && r.IsDeleted == false);
                var tempList = from ment in payMents
                               join paygroupment in groupPayments on ment.Id equals paygroupment.PayMentId
                               select new PayMentRedisModel
                               {
                                   Id = ment.Id,
                                   Name = ment.Name,
                                   Type = ment.Type,
                                   CompanyType = ment.CompanyType,
                                   Gateway = ment.Gateway,
                                   CompanyKey = ment.CompanyKey,
                                   CompanySecret = ment.CompanySecret,
                                   BusinessType = ment.BusinessType,
                                   FullName = ment.FullName,
                                   Phone = ment.Phone,
                                   Mail = ment.Mail,
                                   QrCode = ment.QrCode,
                                   PassWord = ment.PassWord,
                                   CardNumber = ment.CardNumber,
                                   MinMoney = ment.MinMoney,
                                   MaxMoney = ment.MaxMoney,
                                   LimitMoney = ment.LimitMoney,
                                   BalanceLimitMoney = ment.BalanceLimitMoney,
                                   UseMoMo = ment.UseMoMo,
                                   DispenseType = ment.DispenseType,
                                   Remark = ment.Remark,
                                   IsDeleted = ment.IsDeleted,
                                   CreationTime = ment.CreationTime,
                                   ShowStatus = ment.Status,
                                   UseStatus = paygroupment.Status,
                                   MoMoRate = ment.MoMoRate,
                                   ZaloRate = ment.ZaloRate,
                                   VittelPayRate = ment.VittelPayRate,
                                   CryptoWalletAddress = ment.CryptoWalletAddress,
                                   CryptoMinConversionRate = ment.CryptoMinConversionRate,
                                   CryptoMaxConversionRate = ment.CryptoMaxConversionRate,
                               };
                var payMentRedisModel = ObjectMapper.Map<List<PayMentRedisModel>>(payMents);
                PayGroupMentRedisModel redisModel = new PayGroupMentRedisModel()
                {
                    GroupId = payGroup.Id,
                    GroupName = payGroup.GroupName,
                    BankApi = payGroup.BankApi,
                    VietcomApi = payGroup.VietcomApi,
                    PayMents = tempList.ToList()
                };
                _redisService.AddPayGroupMentRedisValue(payGroup.Id.ToString(), redisModel);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayMents)]
        public async Task AddPayGroupMentRedisValueByPayMentId(int paymentId)
        {
            var groupPayments = await _payGroupMentRepository.GetAllListAsync(r => r.PayMentId == paymentId && r.IsDeleted == false);
            foreach (var item in groupPayments)
            {
                await AddPayGroupMentRedisValue(item.GroupId);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayMents)]
        public void AddPayMentRedisValue(PayMentDto payMent)
        {
            PayMentRedisModel payMentRedisModel = new PayMentRedisModel()
            {
                Id = payMent.Id,
                Name = payMent.Name,
                Type = payMent.Type,
                CompanyType = payMent.CompanyType,
                Gateway = payMent.Gateway,
                CompanyKey = payMent.CompanyKey,
                CompanySecret = payMent.CompanySecret,
                BusinessType = payMent.BusinessType,
                FullName = payMent.FullName,
                Phone = payMent.Phone,
                Mail = payMent.Mail,
                QrCode = payMent.QrCode,
                PassWord = payMent.PassWord,
                CardNumber = payMent.CardNumber,
                MinMoney = payMent.MinMoney,
                MaxMoney = payMent.MaxMoney,
                LimitMoney = payMent.LimitMoney,
                BalanceLimitMoney = payMent.BalanceLimitMoney,
                UseMoMo = payMent.UseMoMo,
                DispenseType = payMent.DispenseType,
                Remark = payMent.Remark,
                IsDeleted = payMent.IsDeleted,
                CreationTime = payMent.CreationTime,
                MoMoRate = payMent.MoMoRate,
                ZaloRate = payMent.ZaloRate,
                ShowStatus = payMent.Status,
                VittelPayRate = payMent.VittelPayRate,
                CryptoWalletAddress = payMent.CryptoWalletAddress,
                CryptoMinConversionRate = payMent.CryptoMinConversionRate,
                CryptoMaxConversionRate = payMent.CryptoMaxConversionRate,
            };
            _redisService.SetPayMentCaches(payMentRedisModel);
        }
    }
}
using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Common;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.RedisExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Extensions;

namespace Neptune.NsPay
{
    [RemoteService(false)]
    public class PublicPullAppService : NsPayAppServiceBase, IPublicPullAppService
    {
        private readonly IRepository<PayMent> _payMentRepository;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRedisService _redisService;

        public PublicPullAppService(IRepository<PayMent> payMentRepository,
            IPayOrdersMongoService payOrdersMongoService, IAppConfigurationAccessor appConfigurationAccessor, IRedisService redisService)
        {
            _payMentRepository = payMentRepository;
            _payOrdersMongoService = payOrdersMongoService;
            _appConfiguration = appConfigurationAccessor.Configuration;
            _redisService = redisService;
        }


        #region PayOrder
        public virtual async Task<PagedResultDto<GetPayOrderForViewDto>> GetAll(GetAllPayOrdersInput input)
        {
            var countryCode = "";
            if (input.UtcTimeFilter == "GMT8+")
            {
                countryCode = CultureTimeHelper.TimeCodeZhCN;
            }
            if (input.UtcTimeFilter == "GMT7+")
            {
                countryCode = CultureTimeHelper.TimeCodeViVn;
            }
            if (input.UtcTimeFilter == "GMT4-")
            {
                countryCode = CultureTimeHelper.TimeCodeEST;
            }
            if (countryCode.IsNullOrEmpty())
            {
                countryCode = CultureTimeHelper.TimeCodeZhCN;
            }

            var merchantNameDict = new Dictionary<string, string>();
            List<int> merchantIds = new List<int>();

            string merchantName = string.Empty;

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
            {
                var merchantInfo = _redisService.GetMerchantKeyValue(input.MerchantCodeFilter);

                if (merchantInfo != null)
                {
                    merchantNameDict.Add(merchantInfo.MerchantCode, merchantInfo.Name);
                    merchantIds.Add(merchantInfo.Id);
                    merchantName = merchantInfo.Name;
                }
            }
            else if (input.MerchantIds?.Count > 0)
            {
                merchantIds = new List<int>(input.MerchantIds);
            }

            try
            {
                var filteredPayOrders = await _payOrdersMongoService.GetAllWithPagination(input, merchantIds);

                if (filteredPayOrders != null)
                {
                    var results = new List<GetPayOrderForViewDto>();
                    CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);
                    var payMents = new List<PayMent>();
                    payMents = _payMentRepository.GetAll().ToList();
                    foreach (var o in filteredPayOrders.Items)
                    {

                        if (!merchantNameDict.ContainsKey(o.MerchantCode))
                        {
                            var merchantInfo = _redisService.GetMerchantKeyValue(o.MerchantCode);

                            if (merchantInfo != null)
                            {
                                merchantNameDict.Add(o.MerchantCode, merchantInfo.Name);
                            }
                        }

                        var res = new GetPayOrderForViewDto()
                        {
                            PayOrder = new PayOrderDto
                            {
                                MerchantCode = o.MerchantCode,
                                OrderNo = o.OrderNo,
                                TransactionNo = o.TransactionNo,
                                OrderType = o.OrderType,
                                OrderStatus = o.OrderStatus,
                                OrderMoney = o.OrderMoney.ToString("C0", culInfo),
                                Rate = o.Rate,
                                FeeMoney = o.FeeMoney.ToString("C0", culInfo),
                                OrderTime = o.OrderTime,
                                TransactionTime = CultureTimeHelper.GetCultureTimeInfo(o.TransactionTime, countryCode),
                                CreationTime = CultureTimeHelper.GetCultureTimeInfo(o.CreationTime, countryCode),
                                OrderMark = o.OrderMark,
                                OrderNumber = o.OrderNumber,
                                PlatformCode = o.PlatformCode,
                                ScCode = o.ScCode,
                                ScSeri = o.ScSeri,
                                ScoreStatus = o.ScoreStatus,
                                PayTypeStr = ((PayMentTypeEnum)o.PayType).ToString(),
                                PaymentChannel = o.PaymentChannel,
                                ErrorMsg = o.ErrorMsg,
                                UserNo = o.UserNo,
                                Id = o.ID,
                                Remark = o.Remark,
                            },
                            MerchantName = merchantNameDict[o.MerchantCode],
                            PayMent = ObjectMapper.Map<PayMentDto>(payMents.FirstOrDefault(r => r.Id == o.PayMentId))
                        };

                        results.Add(res);
                    }

                    return new PayOrderPageResultDto<GetPayOrderForViewDto>()
                    {
                        FeeMoneyTotal = filteredPayOrders.FeeMoneyTotal,
                        OrderMoneyTotal = filteredPayOrders.OrderMoneyTotal,
                        TotalCount = filteredPayOrders.TotalCount,
                        Items = results,
                    };
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error(ex.Message);
            }


            return null;
        }
        #endregion


    }
}

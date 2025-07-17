using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Extensions;
using Abp.Json;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Common;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Dto;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.MerchantBills.Exporting;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrders.Exporting;
using Neptune.NsPay.RedisExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune.NsPay.MerchantBills
{
    [AbpAuthorize(AppPermissions.Pages_MerchantBills)]
    public class MerchantBillsAppService : NsPayAppServiceBase, IMerchantBillsAppService
    {
        private readonly IMerchantBillsMongoService _merchantBillsMongoService;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRedisService _redisService;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly MerchantBillsExcelJob _merchantBillExcelJob;
        private readonly IMerchantBillsExcelExporter _merchantBillExcelExporter;

        public MerchantBillsAppService(
            IMerchantBillsMongoService merchantBillsMongoService,
            IBackgroundJobManager backgroundJobManager,
            IRedisService redisService,
            IAppConfigurationAccessor appConfigurationAccessor,
            MerchantBillsExcelJob merchantBillExcelJob,
            IMerchantBillsExcelExporter merchantBillExcelExporter)
        {
            _merchantBillsMongoService = merchantBillsMongoService;
            _backgroundJobManager = backgroundJobManager;
            _redisService = redisService;
            _appConfiguration = appConfigurationAccessor.Configuration;
            _merchantBillExcelJob = merchantBillExcelJob;
            _merchantBillExcelExporter = merchantBillExcelExporter;
        }

        public virtual async Task<PagedResultDto<GetMerchantBillForViewDto>> GetAll(GetAllMerchantBillsInput input)
        {
            var countryCode = input.UtcTimeFilter switch
            {
                "GMT8+" => CultureTimeHelper.TimeCodeZhCN,
                "GMT7+" => CultureTimeHelper.TimeCodeViVn,
                "GMT4-" => CultureTimeHelper.TimeCodeEST,
                _ => CultureTimeHelper.TimeCodeZhCN
            };

            var user = await GetCurrentUserAsync();

            var merchantDict = user.Merchants.ToDictionary(r => r.Id, r => r.Name);
            var userMerchantIdsList = merchantDict.Keys.ToList();

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
            {
                var merchantInfo = user.Merchants.FirstOrDefault(r => r.MerchantCode == input.MerchantCodeFilter);

                if (merchantInfo != null)
                {
                    input.MerchantCodeFilter = string.Empty;
                    userMerchantIdsList = new List<int>() { merchantInfo.Id };
                }
            }

            var filteredMerchantBills = await _merchantBillsMongoService.GetAllWithPagination(input, userMerchantIdsList);
            if (filteredMerchantBills != null)
            {
                CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

                var result = filteredMerchantBills.Items
                    .Select(o => new GetMerchantBillForViewDto
                    {
                        MerchantBill = new MerchantBillDto
                        {
                            MerchantCode = o.MerchantCode,
                            BillNo = o.BillNo,
                            BillType = o.BillType,
                            Money = o.Money.ToString("C0", culInfo),
                            Rate = o.Rate,
                            FeeMoney = o.FeeMoney,
                            BalanceBefore = o.BalanceBefore.ToString("C0", culInfo),
                            BalanceAfter = o.BalanceAfter.ToString("C0", culInfo),
                            PlatformCode = o.PlatformCode,
                            CreationTime = CultureTimeHelper.GetCultureTimeInfo(o.OrderTime, countryCode),
                            TransactionTime = CultureTimeHelper.GetCultureTimeInfo(o.TransactionTime, countryCode),
                            Id = o.ID,
                            Remark = o.Remark,
                        },
                        MerchantName = merchantDict.ContainsKey(o.MerchantId) ? merchantDict[o.MerchantId] : string.Empty
                    })
                    .ToList();

                return new PagedResultDto<GetMerchantBillForViewDto>(filteredMerchantBills.TotalCount, result);
            }

            return null;
        }

        public virtual async Task<PagedResultDto<GetMerchantBillForViewDto>> GetAllCrypto(GetAllMerchantBillsInput input)
        {
            var countryCode = input.UtcTimeFilter switch
            {
                "GMT8+" => CultureTimeHelper.TimeCodeZhCN,
                "GMT7+" => CultureTimeHelper.TimeCodeViVn,
                "GMT4-" => CultureTimeHelper.TimeCodeEST,
                _ => CultureTimeHelper.TimeCodeZhCN
            };

            var user = await GetCurrentUserAsync();

            var merchantDict = user.Merchants.ToDictionary(r => r.Id, r => r.Name);
            var userMerchantIdsList = merchantDict.Keys.ToList();

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
            {
                var merchantInfo = user.Merchants.FirstOrDefault(r => r.MerchantCode == input.MerchantCodeFilter);

                if (merchantInfo != null)
                {
                    input.MerchantCodeFilter = string.Empty;
                    userMerchantIdsList = new List<int>() { merchantInfo.Id };
                }
            }

            var filteredMerchantBills = await _merchantBillsMongoService.GetAllWithPagination(input, userMerchantIdsList, [PayMentMethodEnum.USDTCrypto]);
            if (filteredMerchantBills != null)
            {
                CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

                var result = filteredMerchantBills.Items
                    .Select(o => new GetMerchantBillForViewDto
                    {
                        MerchantBill = new MerchantBillDto
                        {
                            MerchantCode = o.MerchantCode,
                            BillNo = o.BillNo,
                            BillType = o.BillType,
                            Money = $"{o.Money} ₮",
                            Rate = o.Rate,
                            FeeMoney = o.FeeMoney,
                            BalanceBefore = $"{o.BalanceBefore} ₮",
                            BalanceAfter = $"{o.BalanceAfter} ₮",
                            PlatformCode = o.PlatformCode,
                            CreationTime = CultureTimeHelper.GetCultureTimeInfo(o.OrderTime, countryCode),
                            TransactionTime = CultureTimeHelper.GetCultureTimeInfo(o.TransactionTime, countryCode),
                            Id = o.ID,
                            Remark = o.Remark,
                        },
                        MerchantName = merchantDict.ContainsKey(o.MerchantId) ? merchantDict[o.MerchantId] : string.Empty
                    })
                    .ToList();

                return new PagedResultDto<GetMerchantBillForViewDto>(filteredMerchantBills.TotalCount, result);
            }

            return null;
        }

        public virtual async Task<GetMerchantBillForViewDto> GetMerchantBillForView(string id)
        {
            var merchantBill = await _merchantBillsMongoService.GetById(id);

            var output = new GetMerchantBillForViewDto { MerchantBill = ObjectMapper.Map<MerchantBillDto>(merchantBill) };

            return output;
        }

        //[AbpAuthorize(AppPermissions.Pages_MerchantBills_Create)]
        //public virtual async Task<GetMerchantBillForEditOutput> GetMerchantBillForEdit(EntityDto<string> input)
        //{
        //    var merchantBill = await _merchantBillsMongoService.GetById(input.Id);

        //    var output = new GetMerchantBillForEditOutput { MerchantBill = ObjectMapper.Map<CreateOrEditMerchantBillDto>(merchantBill) };

        //    return output;
        //}

        //public virtual async Task CreateOrEdit(CreateOrEditMerchantBillDto input)
        //{
        //	if (input.Id == null)
        //	{
        //		await Create(input);
        //	}
        //}

        //[AbpAuthorize(AppPermissions.Pages_MerchantBills_Create)]
        //protected virtual async Task Create(CreateOrEditMerchantBillDto input)
        //{
        //          var merchant = _redisService.GetMerchantKeyValue(input.MerchantCode);

        //          if (merchant != null)
        //          {
        //		var user = await GetCurrentUserAsync();

        //		if (input.BillType == MerchantBillTypeEnum.AddBill)
        //              {
        //                  MerchantBillsMongoEntity merchantBillsMongo = new MerchantBillsMongoEntity()
        //                  {
        //                      MerchantCode = input.MerchantCode,
        //                      MerchantId = merchant.Id,
        //                      BillNo = Guid.NewGuid().ToString("N"),
        //                      BillType = MerchantBillTypeEnum.AddBill,
        //                      Money = input.Money,
        //                      Rate = 0,
        //                      FeeMoney = 0,
        //                      Remark = input.Remark + "   " + "商户加款，用户：" + user.UserName + ",操作订单强制成功时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        //                  };
        //                  await _merchantBillsMongoService.ArtificialMerchantWithdrawBill(input.MerchantCode, merchantBillsMongo);
        //              }

        //              if (input.BillType == MerchantBillTypeEnum.DeleteBill)
        //              {
        //                  MerchantBillsMongoEntity merchantBillsMongo = new MerchantBillsMongoEntity()
        //                  {
        //                      MerchantCode = input.MerchantCode,
        //                      MerchantId = merchant.Id,
        //                      BillNo = Guid.NewGuid().ToString("N"),
        //                      BillType = MerchantBillTypeEnum.DeleteBill,
        //                      Money = input.Money,
        //                      Rate = 0,
        //                      FeeMoney = 0,
        //				Remark = input.Remark + "   " + "商户扣款，用户：" + user.UserName + ",操作订单强制成功时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        //			};
        //			await _merchantBillsMongoService.ArtificialMerchantWithdrawBill(input.MerchantCode, merchantBillsMongo);
        //		}
        //          }
        //}

        public virtual async Task<string> GetMerchantBillsToExcel(GetAllMerchantBillsForExcelInput input)
        {
            var user = await GetCurrentUserAsync();
            input.userMerchantIdsList = user.Merchants.Select(r => r.Id).ToList();
            var inputStr = input.ToJsonString();
            var cacheInput = _redisService.GetMerchantBillsExcel(user.UserName);
            FileDto file = null;

            var excelRecord = await _merchantBillExcelJob.GetTotalRecordExcel(input);

            if (inputStr != cacheInput)
            {
                if (excelRecord < 5000)
                {
                    var orderLists = await _merchantBillExcelJob.GetMerchantBillsExportAsync(user.ToUserIdentifier(), input);

                    var pageNumber = 1;
                    var rowCount = 1000000;

                    if (orderLists.Count == 0)
                    {
                        file = _merchantBillExcelExporter.ExportToFile(new List<GetMerchantBillForViewDto>(), pageNumber);
                    }
                    else
                    {
                        for (int i = 0; i < orderLists.Count; i += rowCount)
                        {
                            int currentBatchSize = Math.Min(rowCount, orderLists.Count - i);

                            file = _merchantBillExcelExporter.ExportToFile(orderLists.GetRange(i, currentBatchSize), pageNumber);

                            pageNumber++;
                        }
                    }
                    return string.Format(NsPayConsts.FileDownloadExcelPath,
                                          file.FileToken,
                                          Uri.EscapeDataString(file.FileType),
                                          Uri.EscapeDataString(file.FileName));
                }
                else
                {
                    MerchantBillsExcelJobArgs args = new MerchantBillsExcelJobArgs()
                    {
                        input = input,
                        User = user.ToUserIdentifier(),
                    };
                    _redisService.SetMerchantBillsExcel(user.UserName, inputStr);
                    await _backgroundJobManager.EnqueueAsync<MerchantBillsExcelJob, MerchantBillsExcelJobArgs>(args);
                }
            }
            return "";
        }

        public async Task<IList<GetOrderMerchantViewDto>> GetOrderMerchants()
        {
            var user = await GetCurrentUserAsync();
            var merchants = user.Merchants.Select(r => new GetOrderMerchantViewDto
            {
                MerchantCode = r.MerchantCode,
                MerchantName = r.Name
            }).ToList();
            return merchants;
        }

        public bool IsShowMerchantFilter()
        {
            var user = GetCurrentUser();

            if (user.UserType == UserTypeEnum.NsPayAdmin || user.UserType == UserTypeEnum.NsPayKefu)
            {
                return true;
            }
            return false;
        }

        public async Task<List<MerchantDto>> GetMerchants()
        {
            //获取当前的商户
            var user = await GetCurrentUserAsync();
            List<MerchantDto> merchantDtos = new List<MerchantDto>();
            var merchants = user.Merchants.Where(r => r.MerchantType == MerchantTypeEnum.External);
            foreach (var item in user.Merchants)
            {
                var merchantdto = ObjectMapper.Map<MerchantDto>(item);
                merchantDtos.Add(merchantdto);
            }
            return merchantDtos;
        }
    }
}
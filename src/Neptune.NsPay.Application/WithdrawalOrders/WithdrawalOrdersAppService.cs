using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Json;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Common;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Dto;
using Neptune.NsPay.ELKLogExtension;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PlatfromServices.AppServices.Interfaces;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.Storage;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.WithdrawalDevices.Dtos;
using Neptune.NsPay.WithdrawalOrders.Dtos;
using Neptune.NsPay.WithdrawalOrders.Exporting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Neptune.NsPay.WithdrawalOrders
{
    [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders)]
    public class WithdrawalOrdersAppService : NsPayAppServiceBase, IWithdrawalOrdersAppService
    {
        private readonly IRepository<WithdrawalDevice> _withdrawalDeviceRepository;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IWithdrawalOrdersExcelExporter _withdrawalOrdersExcelExporter;
        private readonly ITransferCallBackService _transferCallBackService;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRedisService _redisService;
        private readonly IRepository<BinaryObject, Guid> _binaryObjectRepository;
        private readonly WithdrawalOrderExcelJob _withdrawalOrderExcelJob;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IMerchantFundsMongoService _merchantFundsMongoService;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly LogOrderService _logOrderService;

        public WithdrawalOrdersAppService(
            IRepository<WithdrawalDevice> withdrawalDeviceRepository,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService,
            IWithdrawalOrdersExcelExporter withdrawalOrdersExcelExporter,
            IAppConfigurationAccessor appConfigurationAccessor,
            ITransferCallBackService transferCallBackService,
            IRedisService redisService,
            IRepository<BinaryObject, Guid> binaryObjectRepository,
            WithdrawalOrderExcelJob withdrawalOrderExcelJob,
            IBackgroundJobManager backgroundJobManager,
            IMerchantFundsMongoService merchantFundsMongoService,
            IKafkaProducer kafkaProducer,
            LogOrderService logOrderService
        )
        {
            _withdrawalDeviceRepository = withdrawalDeviceRepository;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _withdrawalOrdersExcelExporter = withdrawalOrdersExcelExporter;
            _appConfiguration = appConfigurationAccessor.Configuration;
            _transferCallBackService = transferCallBackService;
            _redisService = redisService;
            _binaryObjectRepository = binaryObjectRepository;
            _withdrawalOrderExcelJob = withdrawalOrderExcelJob;
            _backgroundJobManager = backgroundJobManager;
            _merchantFundsMongoService = merchantFundsMongoService;
            _kafkaProducer = kafkaProducer;
            _logOrderService = logOrderService;
        }

        private string GetCountryCode(string timeZone)
        {
            switch (timeZone)
            {
                case "GMT8+":
                    return CultureTimeHelper.TimeCodeZhCN;

                case "GMT7+":
                    return CultureTimeHelper.TimeCodeViVn;

                case "GMT4+":
                    return CultureTimeHelper.TimeCodeEST;

                default:
                    return CultureTimeHelper.TimeCodeZhCN;
            }
        }

        public virtual async Task<WithdrawalOrderPageResultDto<GetWithdrawalOrderForViewDto>> GetAll(GetAllWithdrawalOrdersInput input)
        {
            var user = await GetCurrentUserAsync();
            var merchant = user.Merchants.ToDictionary(x => x.Id, x => x.Name);

            var deviceBankType = input.WithdrawalDeviceBankTypeFilter.HasValue
            ? (WithdrawalDevicesBankTypeEnum)input.WithdrawalDeviceBankTypeFilter
            : default;

            var devices = new List<int>();
            if (!string.IsNullOrEmpty(input.WithdrawalDevicePhoneFilter))
            {
                devices = (await _withdrawalDeviceRepository.GetAllListAsync(r => r.Name.Contains(input.WithdrawalDevicePhoneFilter))).Select(r => r.Id).ToList();
            }
            if (input.WithdrawalDeviceBankTypeFilter.HasValue && input.WithdrawalDeviceBankTypeFilter > -1)
            {
                var temps = (await _withdrawalDeviceRepository.GetAllListAsync(r => r.BankType == deviceBankType && r.IsDeleted == false)).Select(r => r.Id).ToList();
                devices.AddRange(temps);
            }
            var filteredWithdrawalOrders = await _withdrawalOrdersMongoService.GetAllWithPagination(input, merchant.Keys.ToList(), devices);

            if (filteredWithdrawalOrders != null)
            {
                var results = new List<GetWithdrawalOrderForViewDto>();
                var withdrawalDevices = _withdrawalDeviceRepository.GetAll().ToDictionary(x => x.Id, x => x);
                CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

                results.AddRange(filteredWithdrawalOrders.Items.Select(o => new GetWithdrawalOrderForViewDto()
                {
                    WithdrawalOrder = new WithdrawalOrderDto
                    {
                        MerchantCode = o.MerchantCode,
                        MerchantId = o.MerchantId,
                        PlatformCode = o.PlatformCode,
                        WithdrawNo = o.WithdrawNo,
                        OrderNo = o.OrderNumber,
                        TransactionNo = o.TransactionNo,
                        OrderStatus = o.OrderStatus,
                        OrderMoney = o.OrderMoney.ToString("C0", culInfo),
                        OrderMoneyDec = o.OrderMoney,
                        Rate = o.Rate,
                        FeeMoney = o.FeeMoney.ToString("C0", culInfo),
                        DeviceId = o.DeviceId,
                        NotifyStatus = o.NotifyStatus,
                        BenAccountName = o.BenAccountName,
                        BenBankName = o.BenBankName,
                        BenAccountNo = o.BenAccountNo,
                        CreationTime = CultureTimeHelper.GetCultureTimeInfo(o.CreationTime, GetCountryCode(input.UtcTimeFilter)),
                        TransactionTime = CultureTimeHelper.GetCultureTimeInfo(o.TransactionTime, GetCountryCode(input.UtcTimeFilter)),
                        Remark = o.Remark,
                        Id = o.ID,
                        HaveProof = (!o.ProofContent.IsNullOrEmpty() || o.BinaryContentId.HasValue),
                        IsShowSuccessCallBack = o.CreationUnixTime > 0 && (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (o.CreationUnixTime / 1000)) < (48 * 60 * 60),
                        IsManualPayout = o.IsManualPayout,
                        ReleaseStatus = o.ReleaseStatus,
                    },
                    MerchantName = merchant.ContainsKey(o.MerchantId) ? merchant[o.MerchantId] : string.Empty,
                    WithdrawalDevice = ObjectMapper.Map<WithdrawalDeviceDto>(withdrawalDevices.ContainsKey(o.DeviceId) ? withdrawalDevices[o.DeviceId] : null)
                }
                    ));

                return new WithdrawalOrderPageResultDto<GetWithdrawalOrderForViewDto>()
                {
                    Items = results,
                    TotalCount = filteredWithdrawalOrders.TotalCount,
                    FeeMoneyTotal = filteredWithdrawalOrders.FeeMoneyTotal,
                    OrderMoneyTotal = filteredWithdrawalOrders.OrderMoneyTotal,
                };
            }

            return null;
        }

        public virtual async Task<GetWithdrawalOrderForViewDto> GetWithdrawalOrderForView(string id)
        {
            var withdrawalOrder = await _withdrawalOrdersMongoService.GetById(id);

            var output = new GetWithdrawalOrderForViewDto { WithdrawalOrder = ObjectMapper.Map<WithdrawalOrderDto>(withdrawalOrder) };

            return output;
        }

        public virtual async Task<List<GetWithdrawalOrderForViewDto>> GetWithdrawalOrderListForView(List<string> ids)
        {
            CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

            var withdrawalOrderList = await _withdrawalOrdersMongoService.GetListByIds(ids);

            var output = withdrawalOrderList.Select(x => new GetWithdrawalOrderForViewDto
            {
                WithdrawalOrder = new WithdrawalOrderDto
                {
                    MerchantCode = x.MerchantCode,
                    MerchantId = x.MerchantId,
                    PlatformCode = x.PlatformCode,
                    WithdrawNo = x.WithdrawNo,
                    OrderNo = x.OrderNumber,
                    TransactionNo = x.TransactionNo,
                    OrderStatus = x.OrderStatus,
                    OrderMoney = x.OrderMoney.ToString("C0", culInfo),
                    OrderMoneyDec = x.OrderMoney,
                    Rate = x.Rate,
                    FeeMoney = x.FeeMoney.ToString("C0", culInfo),
                    DeviceId = x.DeviceId,
                    NotifyStatus = x.NotifyStatus,
                    BenAccountName = x.BenAccountName,
                    BenBankName = x.BenBankName,
                    BenAccountNo = x.BenAccountNo,
                    CreationTime = x.CreationTime,
                    TransactionTime = x.TransactionTime,
                    Remark = x.Remark,
                    Id = x.ID,
                    HaveProof = (!x.ProofContent.IsNullOrEmpty() || x.BinaryContentId.HasValue),
                    IsShowSuccessCallBack = x.CreationUnixTime > 0 && (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (x.CreationUnixTime / 1000)) < (48 * 60 * 60),
                    IsManualPayout = x.IsManualPayout,
                    ReleaseStatus = x.ReleaseStatus,
                }
            }).ToList();

            return output;
        }

        public virtual async Task<GetDisplayProofDto> GetDisplayProofForView(string id)
        {
            var withdrawalOrder = await _withdrawalOrdersMongoService.GetById(id);
            var base64Content = string.Empty;
            var orderNumber = withdrawalOrder.OrderNumber;

            if (!withdrawalOrder.ProofContent.IsNullOrEmpty())
            {
                base64Content = withdrawalOrder.ProofContent;
            }
            else if (withdrawalOrder.BinaryContentId.HasValue)
            {
                var binaryInfo = await _binaryObjectRepository.FirstOrDefaultAsync(x => x.Id == withdrawalOrder.BinaryContentId.Value);

                if (binaryInfo != null)
                {
                    base64Content = string.Format(@"data:{0};base64,{1}", withdrawalOrder.ContentMIMEType, Convert.ToBase64String(binaryInfo.Bytes));
                }
            }

            var output = new GetDisplayProofDto { Base64Content = base64Content, OrderNumber = orderNumber };

            return output;
        }

        public virtual async Task<GetWithdrawalOrderForViewDto> GetWithdrawalOrderForViewPayoutDetails(string id, string utcTimeFilter)
        {
            var withdrawalOrder = await _withdrawalOrdersMongoService.GetById(id);

            var output = new GetWithdrawalOrderForViewDto { WithdrawalOrder = ObjectMapper.Map<WithdrawalOrderDto>(withdrawalOrder) };
            output.WithdrawalOrder.OrderNo = withdrawalOrder.OrderNumber;
            output.WithdrawalOrder.CreationTime = CultureTimeHelper.GetCultureTimeInfo(output.WithdrawalOrder.CreationTime, GetCountryCode(utcTimeFilter));

            var merchantCache = _redisService.GetMerchantKeyValue(output.WithdrawalOrder.MerchantCode);

            if (merchantCache != null)
            {
                output.MerchantName = merchantCache.Name;
            }

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_Edit)]
        public virtual async Task<GetWithdrawalOrderForEditOutput> GetWithdrawalOrderForEdit(EntityDto<string> input)
        {
            var withdrawalOrder = await _withdrawalOrdersMongoService.GetById(input.Id);

            var output = new GetWithdrawalOrderForEditOutput { WithdrawalOrder = ObjectMapper.Map<CreateOrEditWithdrawalOrderDto>(withdrawalOrder) };

            return output;
        }

        public virtual async Task<string> GetWithdrawalOrdersToExcel(GetAllWithdrawalOrdersForExcelInput input)
        {
            var user = await GetCurrentUserAsync();
            input.userMerchantIdsList = user.Merchants.Select(r => r.Id).ToList();
            FileDto file = null;
            var recordExcel = await _withdrawalOrderExcelJob.GetTotalRecordExcel(user.ToUserIdentifier(), input);
            var inputStr = input.ToJsonString();
            var cacheInput = _redisService.GetWithdrawalOrderExcel(user.UserName);

            if (inputStr != cacheInput)
            {
                if (recordExcel <= 5000)
                {
                    var orderLists = await _withdrawalOrderExcelJob.GetWithdrawalOrderExportAsync(user.ToUserIdentifier(), input);

                    var pageNumber = 1;
                    var rowCount = 1000000;
                    if (orderLists.Count == 0)
                    {
                        file = _withdrawalOrdersExcelExporter.ExportToFile(new List<GetWithdrawalOrderForViewDto>(), user.UserType);
                    }
                    else
                    {
                        for (int i = 0; i < orderLists.Count; i += rowCount)
                        {
                            int currentBatchSize = Math.Min(rowCount, orderLists.Count - i);

                            file = _withdrawalOrdersExcelExporter.ExportToFile(orderLists.GetRange(i, currentBatchSize), user.UserType);

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
                    WithdrawalOrderExcelJobArgs args = new WithdrawalOrderExcelJobArgs()
                    {
                        input = input,
                        User = user.ToUserIdentifier(),
                        UserType = user.UserType,
                    };
                    _redisService.SetWithdrawalOrderExcel(user.UserName, inputStr);
                    await _backgroundJobManager.EnqueueAsync<WithdrawalOrderExcelJob, WithdrawalOrderExcelJobArgs>(args);
                }
            }
            return "";
        }

        //手动转账，标记订单完成等待回调
        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_CallBack)]
        public async Task<decimal> GetOrderMerchantBalance(string merchantCode)
        {
            var funds = await _merchantFundsMongoService.GetFundsByMerchantCode(merchantCode);
            if (funds == null)
            {
                return 0;
            }
            else
            {
                return funds.Balance;
            }
        }

        #region EnforceCallBcak

        //手动转账，标记订单完成等待回调
        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_CallBack)]
        public async Task<bool> EnforceCallBcak(EntityDto<string> order)
        {
            var withdrawalOrder = await _withdrawalOrdersMongoService.GetById(order.Id);
            if (withdrawalOrder is null)
                return false;

            var processId = Guid.NewGuid().ToString();
            var currentUser = await GetCurrentUserAsync();

            return await ProcessEnforceCallBcak(processId, currentUser, withdrawalOrder);
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_CallBack)]
        public async Task<BatchCallBackResultDto> BatchEnforceCallBcak(List<EntityDto<string>> orders)
        {
            var result = new BatchCallBackResultDto
            {
                IsSuccess = true,
                SuccessOrder = new List<string>(),
                FailedOrder = new List<string>()
            };

            if (orders is not { Count: > 0 }) return result;

            var processId = Guid.NewGuid().ToString();
            var currentUser = await GetCurrentUserAsync();

            foreach (var order in orders)
            {
                var withdrawalOrder = await _withdrawalOrdersMongoService.GetById(order.Id);
                if (withdrawalOrder is null) continue;

                if (await ProcessEnforceCallBcak(processId, currentUser, withdrawalOrder))
                {
                    result.SuccessOrder.Add(withdrawalOrder.OrderNumber);
                }
                else
                {
                    result.FailedOrder.Add(withdrawalOrder.OrderNumber);
                }
            }

            return result;
        }

        private async Task<bool> ProcessEnforceCallBcak(string processId, User currentUser, WithdrawalOrdersMongoEntity withdrawalOrder)
        {
            if (withdrawalOrder.OrderStatus is WithdrawalOrderStatusEnum.Wait or WithdrawalOrderStatusEnum.Success)
                return false;

            if (withdrawalOrder.OrderStatus is WithdrawalOrderStatusEnum.Pending && !withdrawalOrder.IsManualPayout)
                return false;

            if (withdrawalOrder.NotifyStatus is not WithdrawalNotifyStatusEnum.Wait)
                return false;

            var sw = Stopwatch.StartNew();
            sw.Start();

            withdrawalOrder.Remark += $"用户：{currentUser?.UserName}, " +
                $"订单号：{withdrawalOrder.OrderNumber}, " +
                $"原状态：{withdrawalOrder.OrderStatus}, " +
                $"修改操作【{(withdrawalOrder.IsManualPayout ? "转帐完成" : "手动转账")}】, " +
                $"时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";

            withdrawalOrder.OrderStatus = WithdrawalOrderStatusEnum.Success;
            withdrawalOrder.TransactionTime = DateTime.Now;
            withdrawalOrder.TransactionUnixTime = TimeHelper.GetUnixTimeStamp(withdrawalOrder.TransactionTime);
            withdrawalOrder.TransactionNo = "SD" + withdrawalOrder.OrderNumber;
            withdrawalOrder.NotifyStatus = WithdrawalNotifyStatusEnum.Wait;
            withdrawalOrder.NotifyNumber = 0;

            if (await _withdrawalOrdersMongoService.UpdateAsync(withdrawalOrder))
            {
                var successOrderCache = _redisService.GetWithdrawalSuccessOrder(withdrawalOrder.ID);
                if (successOrderCache.IsNullOrEmpty())
                {
                    _redisService.SetWithdrawalSuccessOrder(withdrawalOrder.ID);

                    await _kafkaProducer.ProduceAsync(KafkaTopics.TransferOrderCallBack, withdrawalOrder.ID, new TransferOrderCallbackPublishDto
                    {
                        ProcessId = processId,
                        WithdrawalOrderId = withdrawalOrder.ID
                    });
                }

                //添加流水
                var checkOrderInfo = _redisService.GetMerchantBillTrasferOrder(withdrawalOrder.MerchantCode, withdrawalOrder.OrderNumber);
                if (string.IsNullOrEmpty(checkOrderInfo))
                {
                    _redisService.SetMerchantBillTrasferOrder(withdrawalOrder.MerchantCode, withdrawalOrder.OrderNumber);
                }

                await _kafkaProducer.ProduceAsync(KafkaTopics.TransferOrder, withdrawalOrder.ID, new TransferOrderPublishDto
                {
                    MerchantCode = withdrawalOrder.MerchantCode,
                    WithdrawalOrderId = withdrawalOrder.ID,
                    TriggerDate = DateTime.Now,
                    OrderStatus = (int)withdrawalOrder.OrderStatus,
                    ProcessId = processId,
                });
            }

            sw.Stop();

            _logOrderService.SubmitLog(new OrderLogModel
            {
                ActionName = ActionNameList.WithdrawalOrderUpdateStatus,
                DeviceId = withdrawalOrder.DeviceId,
                LogDate = DateTime.Now,
                OrderId = withdrawalOrder.ID ?? "",
                OrderNumber = withdrawalOrder.OrderNumber ?? "",
                User = currentUser?.UserName ?? "",
                ProceedId = processId,
                OrderStatus = withdrawalOrder.OrderStatus.ToString() ?? "",
                ProcessingTimeMs = sw.ElapsedMilliseconds,
                OrderCreationDate = withdrawalOrder.CreationTime,
                Desc = "Manual Success Order"
            }).FireAndForgetSafeAsync(ex => { NlogLogger.Error(" EnforceCallBcak ELK Error", ex); });

            return true;
        }

        #endregion EnforceCallBcak

        #region CallBcak

        //重新回调
        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_CallBack)]
        public async Task<bool> CallBcak(EntityDto<string> order)
        {
            //修改上分状态，等待定时器重新回调数据
            var withdrawalOrder = await _withdrawalOrdersMongoService.GetById(order.Id);
            if (withdrawalOrder is null) return false;

            var processId = Guid.NewGuid().ToString();
            var currentUser = await GetCurrentUserAsync();

            return await ProcessCallBcak(processId, currentUser, withdrawalOrder);
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_CallBack)]
        public async Task<BatchCallBackResultDto> BatchCallBcak(List<EntityDto<string>> orders)
        {
            var result = new BatchCallBackResultDto
            {
                IsSuccess = true,
                SuccessOrder = new List<string>(),
                FailedOrder = new List<string>()
            };

            if (orders is not { Count: > 0 }) return result;

            foreach (var order in orders)
            {
                var withdrawalOrder = await _withdrawalOrdersMongoService.GetById(order.Id);
                if (withdrawalOrder is null) continue;

                var processId = Guid.NewGuid().ToString();
                var currentUser = await GetCurrentUserAsync();

                if (await ProcessCallBcak(processId, currentUser, withdrawalOrder))
                {
                    result.SuccessOrder.Add(withdrawalOrder.OrderNumber);
                }
                else
                {
                    result.FailedOrder.Add(withdrawalOrder.OrderNumber);
                }
            }

            return result;
        }

        private async Task<bool> ProcessCallBcak(string processId, User currentUser, WithdrawalOrdersMongoEntity withdrawalOrder)
        {
            if (withdrawalOrder.OrderStatus is not WithdrawalOrderStatusEnum.Success)
                return false;

            withdrawalOrder.NotifyStatus = WithdrawalNotifyStatusEnum.Wait;
            withdrawalOrder.NotifyNumber = 0;
            withdrawalOrder.Remark += $"用户：{currentUser?.UserName}, " +
                $"订单号：{withdrawalOrder.OrderNumber}, " +
                $"修改操作【重新回调】, " +
                $"时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";

            return await _withdrawalOrdersMongoService.UpdateAsync(withdrawalOrder);
        }

        #endregion CallBcak

        #region CallBackCancelOrder

        //回调失败
        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_CallBack)]
        public async Task<bool> CallBackCancelOrder(EntityDto<string> order)
        {
            var withdrawalOrder = await _withdrawalOrdersMongoService.GetById(order.Id);
            if (withdrawalOrder is null)
                return false;

            var processId = Guid.NewGuid().ToString();
            var currentUser = await GetCurrentUserAsync();

            return await ProccessCallBackCancelOrder(processId, currentUser, withdrawalOrder);
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_CallBack)]
        public async Task<BatchCallBackResultDto> BatchCallBackCancelOrder(List<EntityDto<string>> orders)
        {
            var result = new BatchCallBackResultDto
            {
                IsSuccess = true,
                SuccessOrder = new List<string>(),
                FailedOrder = new List<string>()
            };

            if (orders is not { Count: > 0 }) return result;

            var processId = Guid.NewGuid().ToString();
            var currentUser = await GetCurrentUserAsync();

            foreach (var order in orders)
            {
                var withdrawalOrder = await _withdrawalOrdersMongoService.GetById(order.Id);
                if (withdrawalOrder is null) continue;

                if (await ProccessCallBackCancelOrder(processId, currentUser, withdrawalOrder))
                {
                    result.SuccessOrder.Add(withdrawalOrder.OrderNumber);
                }
                else
                {
                    result.FailedOrder.Add(withdrawalOrder.OrderNumber);
                }
            }

            return result;
        }

        private async Task<bool> ProccessCallBackCancelOrder(string processId, User currentUser, WithdrawalOrdersMongoEntity withdrawalOrder)
        {
            if (withdrawalOrder.OrderStatus < WithdrawalOrderStatusEnum.Fail)
                return false;

            var remark = $"用户：{currentUser?.UserName}, " +
                $"订单号：{withdrawalOrder.OrderNumber}, " +
                $"修改操作【回调失败】, " +
                $"时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";

            await _kafkaProducer.ProduceAsync(KafkaTopics.TransferOrderCallBack, withdrawalOrder.ID, new TransferOrderCallbackPublishDto
            {
                ProcessId = processId,
                WithdrawalOrderId = withdrawalOrder.ID,
            });

            return true;
        }

        #endregion CallBackCancelOrder

        //取消订单
        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_Cancel)]
        public async Task CancelOrder(EntityDto<string> input)
        {
            var order = await _withdrawalOrdersMongoService.GetById(input.Id);
            var proceedId = Guid.NewGuid().ToString();
            if (order != null)
            {
                //查询是否在缓存队列中，有的话清除
                //var device = await _withdrawalDeviceRepository.FirstOrDefaultAsync(r => r.Id == order.DeviceId);
                //if (device != null)
                //{
                //    _redisService.CheckAndDeleteTransferOrder(device.BankType.ToString(), device.Phone, order.ID);
                //}
                order.OrderStatus = WithdrawalOrderStatusEnum.Fail;
                var user = await GetCurrentUserAsync();
                bool isReleaseRequired = false;
                order.Remark += "用户：" + user.UserName + ",订单号：" + order.OrderNumber + ",修改操作【取消订单】，时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (order.ReleaseStatus == WithdrawalReleaseStatusEnum.PendingRelease)
                {
                    order.ReleaseStatus = WithdrawalReleaseStatusEnum.Released;
                    isReleaseRequired = true;
                }

                if (await _withdrawalOrdersMongoService.UpdateAsync(order))
                {
                    //var transferOrder = new TransferOrderPublishDto()
                    //{
                    //    MerchantCode = order.MerchantCode,
                    //    WithdrawalOrderId = order.ID,
                    //    TriggerDate = DateTime.Now,
                    //    OrderStatus = (int)order.OrderStatus,
                    //    ProcessId = proceedId,
                    //    IsReleaseAmountNeed = true,
                    //};

                    //_merchantBillsHelper.ReleaseWithdrawalWithAttempt(order.ID, user.UserName).FireAndForgetSafeAsync(ex=>{
                    //    NlogLogger.Error("Release Cancel Order Failed " + proceedId + " Order Id - " + order.ID);
                    //});

                    if (isReleaseRequired)
                    {
                        try
                        {
                            var balance = new MerchantBalancePublishDto()
                            {
                                MerchantCode = order.MerchantCode,
                                Type = MerchantBalanceType.Decrease,
                                Money = Convert.ToInt32(order.OrderMoney),
                                ProcessId = proceedId,
                                Source = BalanceTriggerSource.WithdrawalOrder,
                                TriggerDate = DateTime.Now,
                            };
                            await _kafkaProducer.ProduceAsync<MerchantBalancePublishDto>(KafkaTopics.MerchantBalance, order.ID, balance);
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error("Transfer-Kafka推送失败：" + ex.ToString());
                        }
                    }

                    await _kafkaProducer.ProduceAsync<TransferOrderCallbackPublishDto>(KafkaTopics.TransferOrderCallBack, order.ID, new TransferOrderCallbackPublishDto() { ProcessId = proceedId, WithdrawalOrderId = order.ID });

                    _logOrderService.SubmitLog(new OrderLogModel()
                    {
                        ActionName = ActionNameList.WithdrawalOrderUpdateStatus,
                        DeviceId = order.DeviceId,
                        LogDate = DateTime.Now,
                        OrderId = order.ID,
                        OrderNumber = order.OrderNumber,
                        User = user.UserName,
                        ProceedId = proceedId,
                        OrderStatus = order.OrderStatus.ToString(),
                        ProcessingTimeMs = 0,
                        OrderCreationDate = order.CreationTime,
                        Desc = "Cancel Order"
                    }).FireAndForgetSafeAsync(ex => { NlogLogger.Error(" CancelOrder ELK Error", ex); });
                }
            }
        }

        public bool IsShowDeviceFilter()
        {
            var user = GetCurrentUser();
            var merchants = user.Merchants;

            if (merchants.Count == 1)
            {
                var merchant = merchants[0];
                //78win和j88开启查询
                if (merchant.MerchantCode == "ca4fcf87dc6d0613" || merchant.MerchantCode == "248e296973e84494")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
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

        public async Task<List<WithdrawalDeviceDto>> GetWithdrawalDeviceByMerchantCode(string merchantCode)
        {
            var withdrawalDeviceList = await _withdrawalDeviceRepository.GetAllListAsync(p => p.MerchantCode == merchantCode && p.Process == WithdrawalDevicesProcessTypeEnum.Process);

            var deviceIds = withdrawalDeviceList.Select(r => r.Id).ToList();

            var orderCountList = await _withdrawalOrdersMongoService.GetWithdrawOrderCountByDevice(deviceIds);

            var tempDeviceIdOrder = orderCountList
                .GroupBy(r => r.DeviceId)
                .OrderBy(g => g.Count())
                .Select((g, index) => new { g.Key, Order = index })
                .ToDictionary(x => x.Key, x => x.Order);

            // Order the withdrawalDeviceList based on the dictionary lookup, reduce the O(n2)
            var orderedWithdrawalDeviceList = withdrawalDeviceList
                .OrderBy(device => tempDeviceIdOrder.ContainsKey(device.Id) ? tempDeviceIdOrder[device.Id] : int.MaxValue)
                .ToList();

            var output = ObjectMapper.Map<List<WithdrawalDeviceDto>>(orderedWithdrawalDeviceList);

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_ChangeDevice)]
        public virtual async Task<WithdrawalDeviceResultDto> UpdateWithdrawalOrderDevice(EditWithdrawalOrderDeviceDto input)
        {
            var updateResult = new WithdrawalDeviceResultDto() { IsSuccess = false, FailedOrder = new List<string>(), SuccessOrder = new List<string>() };
            if (input.WithdrawalIds is null)
            {
                return updateResult;
            }

            try
            {
                var largeOrderAmountStr = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.LargeWithdrawalOrderAmount);
                decimal actualLargeOrderAmount = !largeOrderAmountStr.IsNullOrEmpty() && int.TryParse(largeOrderAmountStr, out int _value) ? _value : 50000000;

                foreach (var id in input.WithdrawalIds)
                {
                    var cResult = false;
                    var order = await _withdrawalOrdersMongoService.GetById(id);
                    if (order is null || order.OrderStatus != WithdrawalOrderStatusEnum.Wait || _redisService.IsWithdrawalOrderProcessing(order.ID))
                    {
                        // checking orderstatus not in status wait or push order worker have process the order
                    }
                    else
                    {
                        int originalDeviceId = order.DeviceId;
                        var user = await GetCurrentUserAsync();
                        order.DeviceLog += "原设备Id:" + originalDeviceId + " 现设备Id:" + input.DeviceId;
                        order.Remark += "设备已更改 原设备Id:" + originalDeviceId + " 现设备Id:" + input.DeviceId;
                        order.DeviceId = input.DeviceId;
                        var result = await _withdrawalOrdersMongoService.UpdateAsync(order);
                        if (result)
                        {
                            //金额超过5000W，不加入队列手动转账
                            if (order.OrderMoney <= actualLargeOrderAmount)
                            {
                                var deviceInfo = await _withdrawalDeviceRepository.GetAsync(input.DeviceId);
                                var oldDevice = await _withdrawalDeviceRepository.GetAsync(originalDeviceId);

                                WithdrawalOrderRedisModel oldOrderRedisModel = new WithdrawalOrderRedisModel()
                                {
                                    Id = order.ID,
                                    MerchantCode = order.MerchantCode,
                                    OrderNo = order.OrderNumber,
                                    BenBankName = order.BenBankName,
                                    BenAccountNo = order.BenAccountNo,
                                    BenAccountName = order.BenAccountName,
                                    OrderMoney = order.OrderMoney,
                                    MerchantId = order.MerchantId
                                };

                                var haveRemoved = _redisService.RemoveTransferOrder(oldDevice.BankType.ToString(), oldDevice.Phone, oldOrderRedisModel);
                                NlogLogger.Error(oldDevice.BankType.ToString() + "_" + oldDevice.Phone + " Clean Up Queue " + order.ID + " Result - " + haveRemoved);

                                //加入缓存队列
                                WithdrawalOrderRedisModel orderRedisModel = new WithdrawalOrderRedisModel()
                                {
                                    Id = order.ID,
                                    MerchantCode = order.MerchantCode,
                                    OrderNo = order.OrderNumber,
                                    BenBankName = order.BenBankName,
                                    BenAccountNo = order.BenAccountNo,
                                    BenAccountName = order.BenAccountName,
                                    OrderMoney = order.OrderMoney,
                                    MerchantId = order.MerchantId
                                };
                                _redisService.SetRPushTransferOrder(deviceInfo.BankType.ToString(), deviceInfo.Phone, orderRedisModel);
                            }
                        }
                        cResult = true;
                    }

                    if (cResult)
                    {
                        updateResult.SuccessOrder.Add(order.OrderNumber);
                    }
                    else
                    {
                        updateResult.FailedOrder.Add(order.OrderNumber);
                    }
                }

                updateResult.IsSuccess = true;
            }
            catch (Exception ex)
            {
                NlogLogger.Error("UpdateWithdrawalOrderDevice Error", ex);
            }

            return updateResult;
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_ChangeToPendingStatus)]
        public virtual async Task<bool> ChangeOrderStatusToPending(EntityDto<string> input)
        {
            bool returnResult = false;
            var order = await _withdrawalOrdersMongoService.GetById(input.Id);
            //只有等待支付才能更改
            if (order != null && order.OrderStatus == WithdrawalOrderStatusEnum.Wait)
            {
                returnResult = true;
                order.OrderStatus = WithdrawalOrderStatusEnum.Pending;
                order.IsManualPayout = true;
                var user = await GetCurrentUserAsync();
                order.Remark += "用户：" + user.UserName + ",订单号：" + order.OrderNumber + ",修改操作【手动出款】，时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await _withdrawalOrdersMongoService.UpdateAsync(order);
            }

            return returnResult;
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_Cancel)]
        public virtual async Task<bool> UpdateCancelWithdrawalOrderDevice(EditWithdrawalOrderDeviceDto input)
        {
            if (input.WithdrawalIds is null)
            {
                return false;
            }
            foreach (var id in input.WithdrawalIds)
            {
                var order = await _withdrawalOrdersMongoService.GetById(id);
                if (order != null)
                {
                    if (order.OrderStatus != WithdrawalOrderStatusEnum.Success)
                    {
                        order.OrderStatus = WithdrawalOrderStatusEnum.Fail;
                        var user = await GetCurrentUserAsync();
                        order.Remark += "用户：" + user.UserName + ",订单号：" + order.OrderNumber + ",修改操作【取消订单】，时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        await _withdrawalOrdersMongoService.UpdateAsync(order);
                    }
                }
            }
            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_WithdrawalOrders_ReleaseBalance)]
        public virtual async Task<bool> ReleaseLockAmount(EntityDto<string> input)
        {
            bool returnResult = false;
            string processId = Guid.NewGuid().ToString();
            var sw = Stopwatch.StartNew();
            var order = await _withdrawalOrdersMongoService.GetById(input.Id);
            var lockedSuccess = false;
            string userName = string.Empty;
            bool isReleaseOrder = false;
            bool isInsertMerchantBills = false;
            //只有等待支付才能更改
            if (order != null && order.ReleaseStatus == WithdrawalReleaseStatusEnum.PendingRelease)
            {
                if (!order.isBilled && order.OrderStatus == WithdrawalOrderStatusEnum.Success)
                {
                    returnResult = true;
                    isInsertMerchantBills = true;

                    await _kafkaProducer.ProduceAsync<TransferOrderPublishDto>(KafkaTopics.TransferOrder, order.ID, new TransferOrderPublishDto()
                    {
                        MerchantCode = order.MerchantCode,
                        WithdrawalOrderId = order.ID,
                        TriggerDate = DateTime.Now,
                        OrderStatus = (int)order.OrderStatus,
                        ProcessId = processId,
                    });

                    // var isSuccess = await _merchantBillsHelper.UpdateWithRetryAddWithdrawalOrderBillAsyncV2(order.MerchantCode , order.ID);
                }
                else if (order.ReleaseStatus == WithdrawalReleaseStatusEnum.PendingRelease)
                {
                    returnResult = true;
                    isReleaseOrder = true;
                    order.ReleaseStatus = WithdrawalReleaseStatusEnum.Released;
                    var user = await GetCurrentUserAsync();
                    userName = user.UserName;
                    order.Remark += "用户：" + user.UserName + ",订单号：" + order.OrderNumber + ",修改操作【手动解冻金额】，时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var releaseSuccess = await _withdrawalOrdersMongoService.UpdateAsync(order);

                    try
                    {
                        var balance = new MerchantBalancePublishDto()
                        {
                            MerchantCode = order.MerchantCode,
                            Type = MerchantBalanceType.Decrease,
                            Money = Convert.ToInt32(order.OrderMoney),
                            TriggerDate = DateTime.Now,
                            Source = BalanceTriggerSource.WithdrawalOrder,
                            ProcessId = processId
                        };
                        await _kafkaProducer.ProduceAsync<MerchantBalancePublishDto>(KafkaTopics.MerchantBalance, order.ID, balance);
                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error("Transfer-Kafka推送失败：" + ex.ToString());
                    }
                    //lockedSuccess = await _merchantBillsHelper.ReleaseWithdrawalWithAttempt(input.Id, user.UserName);
                }
            }
            sw.Stop();
            if (returnResult)
            {
                _logOrderService.SubmitLog(new OrderLogModel()
                {
                    ActionName = ActionNameList.WithdrawalReleaseLockedBalance,
                    DeviceId = order?.DeviceId ?? 0,
                    LogDate = DateTime.Now,
                    OrderId = order?.ID ?? string.Empty,
                    OrderNumber = order?.OrderNumber ?? string.Empty,
                    User = userName ?? "",
                    ProceedId = processId,
                    OrderStatus = order?.OrderStatus.ToString() ?? string.Empty,
                    ProcessingTimeMs = sw.ElapsedMilliseconds,
                    OrderCreationDate = order?.CreationTime ?? DateTime.Now,
                    Desc = "Manual Release Order(" + isReleaseOrder + ") / Merchant Bills (" + isInsertMerchantBills + ")"
                }).FireAndForgetSafeAsync(ex => { NlogLogger.Error(" ReleaseLockAmount ELK Error", ex); });
            }

            return returnResult;
        }
    }

    internal static class TaskExtension
    {
        internal static void FireAndForgetSafeAsync(this Task task, Action<Exception>? errorHandler = null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    errorHandler?.Invoke(ex);
                    // Optional: log the exception
                }
            });
        }
    }
}
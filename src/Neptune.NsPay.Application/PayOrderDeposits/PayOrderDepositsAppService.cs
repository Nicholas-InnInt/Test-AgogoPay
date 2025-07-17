using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Json;
using Microsoft.Extensions.Configuration;
using MongoDB.Entities;
using Neptune.NsPay.Authorization;
using Neptune.NsPay.Authorization.Roles;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Common;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.Configuration;
using Neptune.NsPay.Dto;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.Localization;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrderDeposits.Exporting;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.PayOrders.Exporting;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Neptune.NsPay.PayOrderDeposits
{
    [AbpAuthorize(AppPermissions.Pages_PayOrderDeposits)]
    public class PayOrderDepositsAppService : NsPayAppServiceBase, IPayOrderDepositsAppService
    {
        private readonly IRepository<PayMent> _payMentRepository;
        private readonly IRepository<PayGroupMent> _payGroupMentRepository;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly IPayOrdersMongoService _payOrdersMongoService;

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRedisService _redisService;
        private readonly IConfigurationRoot _appConfiguration;

        private readonly RoleManager _roleManager;
        private readonly PayOrderDepositsExcelJob _payOrderDepositExcelJob;
        private readonly IPayOrderDepositsExcelExporter _payOrderDepositsExcelExporter;
        private readonly IKafkaProducer _kafkaProducer;

        public PayOrderDepositsAppService(
            IRepository<PayMent> payMentRepository,
            IRepository<PayGroupMent> payGroupMentRepository,
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            IPayOrdersMongoService payOrdersMongoService,

            IBackgroundJobManager backgroundJobManager,
            IRedisService redisService,
            IAppConfigurationAccessor appConfigurationAccessor,

            RoleManager roleManager,
            PayOrderDepositsExcelJob payOrderDepositExcelJob,
            IPayOrderDepositsExcelExporter payOrderDepositsExcelExporter,
            IKafkaProducer kafkaProducer)
        {
            _payMentRepository = payMentRepository;
            _payGroupMentRepository = payGroupMentRepository;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _payOrdersMongoService = payOrdersMongoService;

            _backgroundJobManager = backgroundJobManager;
            _redisService = redisService;
            _appConfiguration = appConfigurationAccessor.Configuration;

            _roleManager = roleManager;
            _payOrderDepositExcelJob = payOrderDepositExcelJob;
            _payOrderDepositsExcelExporter = payOrderDepositsExcelExporter;
            _kafkaProducer = kafkaProducer;
        }

        public virtual async Task<PagedResultDto<GetPayOrderDepositForViewDto>> GetAll(GetAllPayOrderDepositsInput input)
        {
            var stopWatch = new Stopwatch();
            var totalStopWatch = new Stopwatch();
            totalStopWatch.Start();
            stopWatch.Start();

            var user = await GetCurrentUserAsync();

            var countryCode = input.UtcTimeFilter switch
            {
                "GMT8+" => CultureTimeHelper.TimeCodeZhCN,
                "GMT7+" => CultureTimeHelper.TimeCodeViVn,
                "GMT4-" => CultureTimeHelper.TimeCodeEST,
                _ => CultureTimeHelper.TimeCodeZhCN
            };

            List<int> payGroups = new List<int>();

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
            {
                var targetMerchantInfo = user.Merchants.Where(r => r.MerchantCode == input.MerchantCodeFilter);
                if (targetMerchantInfo.Any())
                {
                    input.MerchantIds = targetMerchantInfo.Select(r => r.Id).Distinct().ToList();
                    payGroups = targetMerchantInfo.Select(r => r.PayGroupId).ToList();
                    input.MerchantCodeFilter = string.Empty;
                }
            }
            else
            {
                payGroups = user.Merchants.Select(r => r.PayGroupId).ToList();
            }

            var payMentIds = _payGroupMentRepository.GetAll().Where(r => payGroups.Contains(r.GroupId)).Select(r => r.PayMentId).ToList();
            var paymentDict = _payMentRepository.GetAll().Where(r => payMentIds.Contains(r.Id) && !PayMentHelper.GetCryptoList.Contains(r.Type)).ToDictionary(x => x.Id, x => x.Name);

            var merchantCode = user.Merchants.Select(r => r.MerchantCode).ToList();
            var payOrderIds = new List<string>();
            if (!string.IsNullOrEmpty(input.OrderNoFilter))
            {
                payOrderIds = await _payOrdersMongoService.GetPayOrdersByFilter(input.OrderNoFilter, "", merchantCode);
            }
            var payOrderUserNos = new List<string>();
            if (!string.IsNullOrEmpty(input.UserMemberFilter))
            {
                payOrderUserNos = await _payOrdersMongoService.GetPayOrdersByFilter("", input.UserMemberFilter, merchantCode);
            }

            var filteredPayOrderDeposits = await _payOrderDepositsMongoService.GetAllWithPagination(input, paymentDict.Keys.ToList(), payOrderIds, payOrderUserNos);

            stopWatch.Stop();

            Console.WriteLine("Pay order Get Data  time taken " + stopWatch.ElapsedMilliseconds + "ms");

            if (filteredPayOrderDeposits != null)
            {
                stopWatch.Restart();
                var results = new List<GetPayOrderDepositForViewDto>();
                if (filteredPayOrderDeposits.Items.Count > 0)
                {
                    CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

                    var userIdList = filteredPayOrderDeposits.Items.Select(x => x.UserId).Distinct();
                    var users = UserManager.Users.Where(y => userIdList.Contains(y.Id)).ToDictionary(x => x.Id, x => x);

                    var orderIdList = filteredPayOrderDeposits.Items.Select(x => x.OrderId).Distinct().ToList();
                    var orderListDict = (await _payOrdersMongoService.GetPayOrderByOrderIdList(orderIdList)).ToDictionary(x => x.ID, x => x);

                    results.AddRange(filteredPayOrderDeposits.Items.Select(o =>
                    {
                        var orderPayOrderRecord = !o.OrderId.IsNullOrEmpty() && orderListDict.ContainsKey(o.OrderId) ? ObjectMapper.Map<PayOrderDto>(orderListDict[o.OrderId]) : null;

                        if (orderPayOrderRecord != null)
                        {
                            orderPayOrderRecord.TransactionTime = CultureTimeHelper.GetCultureTimeInfo(orderPayOrderRecord.CreationTime, countryCode);
                        }

                        return new GetPayOrderDepositForViewDto()
                        {
                            PayOrderDeposit = new PayOrderDepositDto
                            {
                                RefNo = o.RefNo,
                                PayType = (PayMentTypeEnum)o.PayType,
                                Type = o.Type,
                                Description = o.Description,

                                CreditAmount = o.CreditAmount.ToString("C0", culInfo),
                                DebitAmount = o.DebitAmount.ToString("C0", culInfo),
                                AvailableBalance = o.AvailableBalance.ToString("C0", culInfo),

                                CreditBank = o.CreditBank,
                                CreditAcctNo = o.CreditAcctNo,
                                CreditAcctName = o.CreditAcctName,

                                DebitBank = o.DebitBank,
                                DebitAcctNo = o.DebitAcctNo,
                                DebitAcctName = o.DebitAcctName,

                                TransactionTime = CultureTimeHelper.GetCultureTimeInfo(o.TransactionTime, countryCode),
                                CreationTime = CultureTimeHelper.GetCultureTimeInfo(o.CreationTime, countryCode),
                                OrderId = o.OrderId.IsNullOrEmpty() ? "" : o.OrderId,
                                MerchantId = o.MerchantId,
                                MerchantCode = o.MerchantCode,
                                UserName = o.UserName,
                                AccountNo = o.AccountNo,

                                PayMentName = paymentDict.ContainsKey(o.PayMentId) ? paymentDict[o.PayMentId] : null,
                                RejectRemark = string.IsNullOrEmpty(o.RejectRemark) ? "" : o.RejectRemark,

                                OperateUser = users.ContainsKey(o.UserId) ? users[o.UserId].UserName : null,
                                OperateTime = o.OperateTime.HasValue ? CultureTimeHelper.GetCultureTimeInfo(o.OperateTime.Value, countryCode) : DateTime.MinValue,
                                Id = o.ID,
                            },
                            PayOrder = orderPayOrderRecord,
                            Type = o.Type == "CRDT" ? 1 : 0
                        };
                    }));
                }

                stopWatch.Stop();

                Console.WriteLine("Pay order Mapping  time taken " + stopWatch.ElapsedMilliseconds + "ms");

                totalStopWatch.Stop();

                Console.WriteLine("Pay order Total Service  time taken " + totalStopWatch.ElapsedMilliseconds + "ms");

                return new PagedResultDto<GetPayOrderDepositForViewDto>(
                    filteredPayOrderDeposits.TotalCount,
                    results
                );
            }

            return null;
        }

        public virtual async Task<PagedResultDto<GetPayOrderDepositForViewDto>> GetAllCrypto(GetAllPayOrderDepositsInput input)
        {
            var stopWatch = new Stopwatch();
            var totalStopWatch = new Stopwatch();
            totalStopWatch.Start();
            stopWatch.Start();

            var user = await GetCurrentUserAsync();

            var countryCode = input.UtcTimeFilter switch
            {
                "GMT8+" => CultureTimeHelper.TimeCodeZhCN,
                "GMT7+" => CultureTimeHelper.TimeCodeViVn,
                "GMT4-" => CultureTimeHelper.TimeCodeEST,
                _ => CultureTimeHelper.TimeCodeZhCN
            };

            List<int> payGroups = new List<int>();

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
            {
                var targetMerchantInfo = user.Merchants.Where(r => r.MerchantCode == input.MerchantCodeFilter);
                if (targetMerchantInfo.Any())
                {
                    input.MerchantIds = targetMerchantInfo.Select(r => r.Id).Distinct().ToList();
                    payGroups = targetMerchantInfo.Select(r => r.PayGroupId).ToList();
                    input.MerchantCodeFilter = string.Empty;
                }
            }
            else
            {
                payGroups = user.Merchants.Select(r => r.PayGroupId).ToList();
            }

            var payMentIds = _payGroupMentRepository.GetAll().Where(r => payGroups.Contains(r.GroupId)).Select(r => r.PayMentId).ToList();
            var paymentDict = _payMentRepository.GetAll().Where(r => payMentIds.Contains(r.Id) && PayMentHelper.GetCryptoList.Contains(r.Type)).ToDictionary(x => x.Id, x => x.Name);

            var merchantCode = user.Merchants.Select(r => r.MerchantCode).ToList();
            var payOrderIds = new List<string>();
            if (!string.IsNullOrEmpty(input.OrderNoFilter))
            {
                payOrderIds = await _payOrdersMongoService.GetPayOrdersByFilter(input.OrderNoFilter, "", merchantCode);
            }
            var payOrderUserNos = new List<string>();
            if (!string.IsNullOrEmpty(input.UserMemberFilter))
            {
                payOrderUserNos = await _payOrdersMongoService.GetPayOrdersByFilter("", input.UserMemberFilter, merchantCode);
            }

            var filteredPayOrderDeposits = await _payOrderDepositsMongoService.GetAllWithPagination(input, paymentDict.Keys.ToList(), payOrderIds, payOrderUserNos);

            stopWatch.Stop();

            Console.WriteLine("Pay order Get Data  time taken " + stopWatch.ElapsedMilliseconds + "ms");

            if (filteredPayOrderDeposits != null)
            {
                stopWatch.Restart();
                var results = new List<GetPayOrderDepositForViewDto>();
                if (filteredPayOrderDeposits.Items.Count > 0)
                {
                    CultureInfo culInfo = CultureHelper.GetCultureInfoByChecking(_appConfiguration["Currency"]);

                    var userIdList = filteredPayOrderDeposits.Items.Select(x => x.UserId).Distinct();
                    var users = UserManager.Users.Where(y => userIdList.Contains(y.Id)).ToDictionary(x => x.Id, x => x);

                    var orderIdList = filteredPayOrderDeposits.Items.Select(x => x.OrderId).Distinct().ToList();
                    var orderListDict = (await _payOrdersMongoService.GetPayOrderByOrderIdList(orderIdList)).ToDictionary(x => x.ID, x => x);

                    results.AddRange(filteredPayOrderDeposits.Items.Select(o =>
                    {
                        var orderPayOrderRecord = !o.OrderId.IsNullOrEmpty() && orderListDict.ContainsKey(o.OrderId) ? ObjectMapper.Map<PayOrderDto>(orderListDict[o.OrderId]) : null;

                        if (orderPayOrderRecord != null)
                        {
                            orderPayOrderRecord.TransactionTime = CultureTimeHelper.GetCultureTimeInfo(orderPayOrderRecord.CreationTime, countryCode);
                        }

                        return new GetPayOrderDepositForViewDto()
                        {
                            PayOrderDeposit = new PayOrderDepositDto
                            {
                                RefNo = o.RefNo,
                                PayType = (PayMentTypeEnum)o.PayType,
                                Type = o.Type,
                                Description = o.Description,

                                CreditAmount = $"{o.CreditAmount} ₮",
                                DebitAmount = $"{o.DebitAmount} ₮",
                                AvailableBalance = $"{o.AvailableBalance} ₮",

                                CreditBank = o.CreditBank,
                                CreditAcctNo = o.CreditAcctNo,
                                CreditAcctName = o.CreditAcctName,

                                DebitBank = o.DebitBank,
                                DebitAcctNo = o.DebitAcctNo,
                                DebitAcctName = o.DebitAcctName,

                                TransactionTime = CultureTimeHelper.GetCultureTimeInfo(o.TransactionTime, countryCode),
                                CreationTime = CultureTimeHelper.GetCultureTimeInfo(o.CreationTime, countryCode),
                                OrderId = o.OrderId.IsNullOrEmpty() ? "" : o.OrderId,
                                MerchantId = o.MerchantId,
                                MerchantCode = o.MerchantCode,
                                UserName = o.UserName,
                                AccountNo = o.AccountNo,

                                PayMentName = paymentDict.ContainsKey(o.PayMentId) ? paymentDict[o.PayMentId] : null,
                                RejectRemark = string.IsNullOrEmpty(o.RejectRemark) ? "" : o.RejectRemark,

                                OperateUser = users.ContainsKey(o.UserId) ? users[o.UserId].UserName : null,
                                OperateTime = o.OperateTime.HasValue ? CultureTimeHelper.GetCultureTimeInfo(o.OperateTime.Value, countryCode) : DateTime.MinValue,
                                Id = o.ID,
                            },
                            PayOrder = orderPayOrderRecord,
                            Type = o.Type == "CRDT" ? 1 : 0
                        };
                    }));
                }

                stopWatch.Stop();

                Console.WriteLine("Pay order Mapping  time taken " + stopWatch.ElapsedMilliseconds + "ms");

                totalStopWatch.Stop();

                Console.WriteLine("Pay order Total Service  time taken " + totalStopWatch.ElapsedMilliseconds + "ms");

                return new PagedResultDto<GetPayOrderDepositForViewDto>(
                    filteredPayOrderDeposits.TotalCount,
                    results
                );
            }

            return null;
        }

        public virtual async Task<string> GetPayOrderDepositsToExcel(GetAllPayOrderDepositsForExcelInput input)
        {
            var user = await GetCurrentUserAsync();
            var inputStr = input.ToJsonString();
            var cacheInput = _redisService.GetPayDepositExcel(user.UserName);
            FileDto file = null;

            var countRecord = await _payOrderDepositExcelJob.GetTotalRecordExcel(input, user.ToUserIdentifier());

            if (inputStr != cacheInput)
            {
                if (countRecord <= 5000)
                {
                    var orderLists = await _payOrderDepositExcelJob.GetPayOrderDepositExportAsync(user.ToUserIdentifier(), input);

                    var pageNumber = 1;
                    var rowCount = 1000000;

                    if (orderLists.Count == 0)
                    {
                        file = _payOrderDepositsExcelExporter.ExportToFile(new List<GetPayOrderDepositForViewDto>(), pageNumber);
                    }
                    else
                    {
                        for (int i = 0; i < orderLists.Count; i += rowCount)
                        {
                            int currentBatchSize = Math.Min(rowCount, orderLists.Count - i);

                            file = _payOrderDepositsExcelExporter.ExportToFile(orderLists.GetRange(i, currentBatchSize), pageNumber);

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
                    PayOrderDepositsExcelJobArgs args = new PayOrderDepositsExcelJobArgs()
                    {
                        input = input,
                        User = user.ToUserIdentifier()
                    };
                    _redisService.SetPayDepositExcel(user.UserName, inputStr);
                    await _backgroundJobManager.EnqueueAsync<PayOrderDepositsExcelJob, PayOrderDepositsExcelJobArgs>(args);
                }
            }
            return "";
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrderDeposits_Edit)]
        public async Task<GetAssociatedDepositOrderOutput> GetAssociatedOrder(EntityDto<string?> input, PayMentTypeEnum payType)
        {
            if (!input.Id.IsNullOrEmpty())
            {
                var flag = false;
                var payOrder = await _payOrderDepositsMongoService.GetPayOrderDepositsByBankOrderId(input.Id, payType);
                if (payOrder != null)
                {
                    flag = true;
                }

                if (flag)
                {
                    var output = new GetAssociatedDepositOrderOutput
                    {
                        AssociatedOrder = new AssociatedDepositOrderCallBackDto()
                        {
                            Id = input.Id,
                            PayType = payType,
                        }
                    };

                    return output;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrderDeposits_Edit)]
        public async Task<GetRejectDepositOrderOutput> GetRejectOrder(EntityDto<string?> input, PayMentTypeEnum payType)
        {
            if (!input.Id.IsNullOrEmpty())
            {
                var flag = false;
                var payOrder = await _payOrderDepositsMongoService.GetPayOrderDepositsByBankOrderId(input.Id, payType);
                if (payOrder != null)
                {
                    flag = true;
                }

                if (flag)
                {
                    var output = new GetRejectDepositOrderOutput
                    {
                        RejectOrder = new RejectPayOrderDepositDto()
                        {
                            Id = input.Id,
                            PayType = payType,
                        }
                    };

                    return output;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task AssociatedOrder(AssociatedDepositOrderCallBackDto input)
        {
            if (!input.Id.IsNullOrEmpty())
            {
                await Associated(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrderDeposits_Edit)]
        protected virtual async Task Associated(AssociatedDepositOrderCallBackDto input)
        {
            if (!input.Id.IsNullOrEmpty())
            {
                if (!string.IsNullOrEmpty(input.OrderNumber))
                {
                    var user = await GetCurrentUserAsync();
                    var roleId = await _roleManager.FindByNameAsync("AF1204BAC9004894AC1BB1E4A181CEA1");
                    var checkFlag = false;
                    if (roleId != null)
                    {
                        var roles = await GetCurrentUserRoleAsync();
                        var checkRole = roles.FirstOrDefault(r => r == roleId.Name);
                        if (checkRole != null)
                        {
                            checkFlag = true;
                        }
                    }
                    var userMerchant = user.Merchants.Select(s => s.MerchantCode).ToList();
                    var payOrder = await _payOrdersMongoService.GetPayOrderByOrderNumber(userMerchant, input.OrderNumber);
                    if (payOrder != null)
                    {
                        if (payOrder.OrderStatus != PayOrderOrderStatusEnum.Completed)
                        {
                            var status = 0;
                            PayMentTypeEnum? depositPaytype = null;
                            string depositsId = "";
                            var transactionNo = "";
                            var payMentId = 0;
                            var phone = "";
                            var bankOrder = await _payOrderDepositsMongoService.GetPayOrderDepositsByBankOrderId(input.Id, input.PayType);
                            if (bankOrder != null && bankOrder.OrderId.IsNullOrEmpty())
                            {
                                if (checkFlag)
                                {
                                    if (payOrder.OrderMoney == bankOrder.CreditAmount)
                                    {
                                        status = 1;
                                        depositsId = bankOrder.ID;
                                        depositPaytype = (PayMentTypeEnum)bankOrder.PayType;
                                        transactionNo = bankOrder.RefNo;
                                        payMentId = bankOrder.PayMentId;
                                        phone = bankOrder.UserName;
                                    }
                                }
                                else
                                {
                                    if (payOrder.OrderMoney == bankOrder.CreditAmount && payOrder.OrderMoney <= 10000000)
                                    {
                                        status = 1;
                                        depositsId = bankOrder.ID;
                                        depositPaytype = (PayMentTypeEnum)bankOrder.PayType;
                                        transactionNo = bankOrder.RefNo;
                                        payMentId = bankOrder.PayMentId;
                                        phone = bankOrder.UserName;
                                    }
                                }
                            }

                            if (status == 1)
                            {
                                var flag = false;
                                var lists = _redisService.GetPayOrderDepositSuccessCache();
                                if (lists != null && lists.Count > 0)
                                {
                                    var info = lists.FirstOrDefault(r => r.DepositsId == depositsId);
                                    if (info == null)
                                    {
                                        flag = true;
                                    }
                                }
                                else
                                {
                                    flag = true;
                                }

                                if (flag)
                                {
                                    var transactionId = "QZ" + transactionNo + "-" + phone;
                                    var temptansactionNo = transactionNo + "-" + phone;

                                    var TransactionNo = transactionId;
                                    if (string.IsNullOrEmpty(TransactionNo))
                                    {
                                        if (string.IsNullOrEmpty(payOrder.TransactionNo))
                                        {
                                            TransactionNo = "QZ" + Guid.NewGuid().ToString("N");
                                        }
                                        else
                                        {
                                            TransactionNo = "QZ" + payOrder.TransactionNo;
                                        }
                                    }
                                    var remark = "收款订单,用户：" + user.UserName + ",订单号：" + payOrder.OrderNo + ",操作订单手动上分成功时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                    PayOrderDepositSuccessModel successModel = new PayOrderDepositSuccessModel()
                                    {
                                        Type = 0,
                                        DepositsId = depositsId,
                                        OrderId = payOrder.ID,
                                        Userid = user.Id,
                                        MerchantId = payOrder.MerchantId,
                                        MerchantCode = payOrder.MerchantCode,
                                        TransactionNo = transactionNo,
                                        PayMentId = payMentId,
                                        TradeMoney = payOrder.TradeMoney,
                                        Remark = remark,
                                        CreateTime = DateTime.Now
                                    };
                                    _redisService.SetPayOrderDepositSuccessCache(successModel);

                                    await _payOrderDepositsMongoService.PayOrderDepositAssociated(payOrder.MerchantCode, payOrder.ID, payOrder.OrderMoney, transactionNo, payMentId, depositPaytype.Value, remark, depositsId, payOrder.MerchantId, user.Id);

                                    //添加流水
                                    var checkOrder = _redisService.GetMerchantBillOrder(payOrder.MerchantCode, payOrder.OrderNumber);
                                    if (string.IsNullOrEmpty(checkOrder))
                                    {
                                        _redisService.SetMerchantBillOrder(payOrder.MerchantCode, payOrder.OrderNumber);
                                        //PayMerchantRedisMqDto redisMqDto = new PayMerchantRedisMqDto()
                                        //{
                                        //    PayMqSubType = MQSubscribeStaticConsts.MerchantBillAddBalance,
                                        //    MerchantCode = payOrder.MerchantCode,
                                        //    PayOrderId = payOrder.ID,
                                        //};
                                        //_redisService.SetMerchantMqPublish(redisMqDto);

                                        await _kafkaProducer.ProduceAsync<PayOrderPublishDto>(KafkaTopics.PayOrder, payOrder.ID, new PayOrderPublishDto()
                                        {
                                            MerchantCode = payOrder.MerchantCode,
                                            PayOrderId = payOrder.ID,
                                            TriggerDate = DateTime.Now,
                                            ProcessId = Guid.NewGuid().ToString()
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrderDeposits_Edit)]
        public async Task AssociatedCryptoOrder(AssociatedDepositOrderCallBackDto input)
        {
            if (string.IsNullOrEmpty(input?.Id) || string.IsNullOrEmpty(input?.OrderNumber)) return;

            var user = await GetCurrentUserAsync();

            var userMerchant = user.Merchants.Select(s => s.MerchantCode).ToList();

            var payOrder = await _payOrdersMongoService.GetPayOrderByOrderNumber(userMerchant, input.OrderNumber);
            var payOrderDeposit = await _payOrderDepositsMongoService.GetPayOrderDepositsByBankOrderId(input.Id, input.PayType);
            if (payOrderDeposit is not null && payOrder is { OrderStatus: PayOrderOrderStatusEnum.NotPaid or PayOrderOrderStatusEnum.TimeOut } && payOrderDeposit.CreditAmount == payOrder.OrderMoney)
            {
                var remark = $"收款订单,用户：{user.UserName},订单号：{payOrder.OrderNo},操作订单手动上分成功时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
                await _payOrderDepositsMongoService.PayOrderDepositAssociated(payOrder.MerchantCode, payOrder.ID, payOrder.OrderMoney, payOrderDeposit.RefNo, payOrder.PayMentId, payOrder.PayType, remark, payOrderDeposit.ID, payOrder.MerchantId, user.Id);
                await _kafkaProducer.ProduceAsync(KafkaTopics.PayOrderCrypto, payOrder.MerchantCode, new PayOrderCryptoPublishDto
                {
                    UpdateOrderNumberBillOnly = payOrder.OrderNumber,
                });
            }
        }

        public async Task RejectOrder(RejectPayOrderDepositDto input)
        {
            if (!input.Id.IsNullOrEmpty())
            {
                await Reject(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrderDeposits_Edit)]
        protected virtual async Task Reject(RejectPayOrderDepositDto input)
        {
            if (!input.Id.IsNullOrEmpty())
            {
                var user = await GetCurrentUserAsync();
                var bankOrder = await _payOrderDepositsMongoService.GetPayOrderDepositsByBankOrderIdNoType(input.Id, input.PayType);
                if (bankOrder != null && bankOrder.OrderId.IsNullOrEmpty())
                {
                    var flag = false;
                    var lists = _redisService.GetPayOrderDepositSuccessCache();
                    if (lists != null && lists.Count > 0)
                    {
                        var info = lists.FirstOrDefault(r => r.DepositsId == bankOrder.ID);
                        if (info == null)
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        PayOrderDepositSuccessModel successModel = new PayOrderDepositSuccessModel()
                        {
                            Type = 1,
                            DepositsId = bankOrder.ID,
                            OrderId = "-1",
                            Userid = user.Id,
                            MerchantId = bankOrder.MerchantId,
                            MerchantCode = bankOrder.MerchantCode,
                            PayMentId = bankOrder.PayMentId,
                            Remark = input.RejectRemark,
                            CreateTime = DateTime.Now
                        };
                        _redisService.SetPayOrderDepositSuccessCache(successModel);

                        //更新order
                        await _payOrderDepositsMongoService.PayOrderDepositReject(bankOrder.ID, user.Id, input.RejectRemark);
                    }
                }
            }
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

        [AbpAuthorize(AppPermissions.Pages_PayOrderDeposits_Edit)]
        public async Task BulkRejectOrder(BulkRejectPayOrderDepositDto input)
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (input != null)
            {
                var orderIds = input.OrderIds.FromJsonString<List<string>>().Distinct().ToList();

                if (orderIds.Count != 0)
                {
                    Dictionary<string, PayOrderDepositsMongoEntity> payOrderList = new Dictionary<string, PayOrderDepositsMongoEntity>();
                    var user = await GetCurrentUserAsync();

                    foreach (var chunk in orderIds.Chunk(100))
                    {
                        var result = await _payOrderDepositsMongoService.GetListByIds(chunk.ToList());

                        foreach (var order in result)
                        {
                            if (!payOrderList.ContainsKey(order.ID))
                            {
                                payOrderList.Add(order.ID, order);
                            }
                        }
                    }

                    SemaphoreSlim semaphore = new SemaphoreSlim(5); // Limit to 3 concurrent tasks.
                    var tasks = new List<Task>();

                    foreach (var orderId in orderIds)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await semaphore.WaitAsync();

                                if (payOrderList.ContainsKey(orderId))
                                {
                                    var bankOrder = payOrderList[orderId];
                                    var isOrderExisted = _redisService.PayOrderDepositSuccessCheckExist(bankOrder.OrderId);

                                    if (isOrderExisted)
                                    {
                                        PayOrderDepositSuccessModel successModel = new PayOrderDepositSuccessModel()
                                        {
                                            Type = 1,
                                            DepositsId = bankOrder.ID,
                                            OrderId = "-1",
                                            Userid = user.Id,
                                            MerchantId = bankOrder.MerchantId,
                                            MerchantCode = bankOrder.MerchantCode,
                                            PayMentId = bankOrder.PayMentId,
                                            Remark = user.UserName + ",BulkReject,Time:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                            CreateTime = DateTime.Now
                                        };
                                        _redisService.SetPayOrderDepositSuccessCache(successModel);

                                        //更新order
                                        await _payOrderDepositsMongoService.PayOrderDepositReject(bankOrder.ID, user.Id, successModel.Remark);
                                    }
                                }
                            }
                            catch (Exception ex) { }
                            finally
                            {
                                semaphore.Release();
                            }
                        }));
                    }

                    await Task.WhenAll(tasks);
                }
            }

            sw.Stop();

            NlogLogger.Warn("批量拒接订单用时 ： " + sw.ElapsedMilliseconds + " ms");
        }

        public virtual async Task<GetPayOrderDepositForViewDto> GetPayOrderDepositForView(string id)
        {
            var payOrderDeposit = await _payOrderDepositsMongoService.GetById(id);

            var payMents = _payMentRepository.GetAll().Where(x => x.Id == payOrderDeposit.PayMentId).ToDictionary(x => x.Id, x => x.Name);

            var user = UserManager.Users.FirstOrDefault(y => y.Id == payOrderDeposit.UserId);

            var output = new GetPayOrderDepositForViewDto { PayOrderDeposit = ObjectMapper.Map<PayOrderDepositDto>(payOrderDeposit) };

            if (payOrderDeposit.OrderId != null && payOrderDeposit.OrderId != "-1")
            {
                var payOrder = await _payOrdersMongoService.GetById(payOrderDeposit.OrderId);
                var payOrderOutput = new GetPayOrderForViewDto { PayOrder = ObjectMapper.Map<PayOrderDto>(payOrder) };

                if (payOrderOutput.PayOrder != null)
                {
                    payOrderOutput.PayOrder.TransactionTime = CultureTimeHelper.GetCultureTimeInfo(payOrder.CreationTime, "vi-VN");
                }
                output.PayOrder = payOrderOutput.PayOrder;
            }

            output.PayOrderDeposit.PayType = (PayMentTypeEnum)payOrderDeposit.PayType;
            output.PayOrderDeposit.PayMentName = payMents.ContainsKey(payOrderDeposit.PayMentId) ? payMents[payOrderDeposit.PayMentId] : null;

            if (user != null)
            {
                output.PayOrderDeposit.OperateUser = user.UserName ?? null;
            }
            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_PayOrderDeposits_Edit)]
        public async Task BulkMatchOrder(BulkRejectPayOrderDepositDto input)
        {
            if (input is null) return;

            var orderIds = input.OrderIds.FromJsonString<List<string>>();
            if (orderIds is not { Count: > 0 }) return;

            var payments = _redisService.GetPayMents();
            foreach (var orderId in orderIds)
            {
                var bankOrder = await _payOrderDepositsMongoService.GetById(orderId);
                if (bankOrder != null && bankOrder.OrderId.IsNullOrEmpty())
                {
                    var payment = payments.FirstOrDefault(r => r.Id == bankOrder.PayMentId);
                    if (payment != null)
                    {
                        var merchantCode = "";
                        var user = await GetCurrentUserAsync();
                        if (user.UserType == UserTypeEnum.NsPayKefu || user.UserType == UserTypeEnum.NsPayAdmin)
                        {
                            merchantCode = NsPayRedisKeyConst.NsPay;
                        }
                        else
                        {
                            merchantCode = user.Merchants.FirstOrDefault()?.MerchantCode ?? "";
                        }

                        var payEntity = await _payOrdersMongoService.GetPayOrderByRemark(merchantCode, payment.Type, bankOrder.Description, bankOrder.CreditAmount);
                        if (payEntity != null)
                        {
                            //更新订单，同时增加流水表
                            try
                            {
                                var bankOrderPubModel = new BankOrderPubModel()
                                {
                                    Type = payment.Type,
                                    PayMentId = bankOrder.PayMentId,
                                    PayOrderId = payEntity.ID,
                                    Id = bankOrder.RefNo,
                                    Money = bankOrder.CreditAmount,
                                    Desc = bankOrder.Description
                                };

                                if (payment.Type is PayMentTypeEnum.BusinessTcbBank)
                                {
                                    bankOrderPubModel.Money = decimal.Parse(bankOrderPubModel.Money.ToString("F2"));
                                }

                                var redisKey = payment.Type switch
                                {
                                    PayMentTypeEnum.ACBBank => NsPayRedisKeyConst.ACBBankOrder,
                                    PayMentTypeEnum.BidvBank => NsPayRedisKeyConst.BIDVBankOrder,
                                    PayMentTypeEnum.BusinessMbBank => NsPayRedisKeyConst.BusinessMbBankOrder,
                                    PayMentTypeEnum.BusinessTcbBank => NsPayRedisKeyConst.BusinessTcbBankOrder,
                                    PayMentTypeEnum.BusinessVtbBank => NsPayRedisKeyConst.BusinessVtbBankOrder,
                                    PayMentTypeEnum.MBBank => NsPayRedisKeyConst.MBBankOrder,
                                    PayMentTypeEnum.PVcomBank => NsPayRedisKeyConst.PVcomBankOrder,
                                    PayMentTypeEnum.TechcomBank => NsPayRedisKeyConst.TCBBankOrder,
                                    PayMentTypeEnum.VietcomBank => NsPayRedisKeyConst.VCBBankOrder,
                                    PayMentTypeEnum.VietinBank => NsPayRedisKeyConst.VTBBankOrder,
                                    PayMentTypeEnum.MsbBank => NsPayRedisKeyConst.MsbBankOrder,
                                    PayMentTypeEnum.SeaBank => NsPayRedisKeyConst.SeaBankOrder,
                                    PayMentTypeEnum.BvBank => NsPayRedisKeyConst.BvBankOrder,
                                    PayMentTypeEnum.NamaBank => NsPayRedisKeyConst.NamaBankOrder,
                                    PayMentTypeEnum.TPBank => NsPayRedisKeyConst.TPBankOrder,
                                    PayMentTypeEnum.VPBBank => NsPayRedisKeyConst.VPBBankOrder,
                                    PayMentTypeEnum.OCBBank => NsPayRedisKeyConst.OCBBankOrder,
                                    PayMentTypeEnum.EXIMBank => NsPayRedisKeyConst.EXIMBankOrder,
                                    PayMentTypeEnum.NCBBank => NsPayRedisKeyConst.NCBBankOrder,
                                    PayMentTypeEnum.HDBank => NsPayRedisKeyConst.HDBankOrder,
                                    PayMentTypeEnum.LPBank => NsPayRedisKeyConst.LPBankOrder,
                                    PayMentTypeEnum.PGBank => NsPayRedisKeyConst.PGBankOrder,
                                    PayMentTypeEnum.VietBank => NsPayRedisKeyConst.VietBankOrder,
                                    PayMentTypeEnum.BacaBank => NsPayRedisKeyConst.BacaBankOrder,
                                    _ => null
                                };

                                if (!string.IsNullOrEmpty(redisKey))
                                {
                                    _redisService.AddOrderQueueList(redisKey, bankOrderPubModel);
                                }
                            }
                            catch (Exception ex)
                            {
                                NlogLogger.Warn("批量回调更新订单流水表异常：" + ex.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
}
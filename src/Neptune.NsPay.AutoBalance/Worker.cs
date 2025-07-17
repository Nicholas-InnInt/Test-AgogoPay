using Abp.Extensions;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.HttpExtensions.Bank.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;

namespace Neptune.NsPay.AutoBalance
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IRedisService _redisService;
        private readonly IVietcomBankHelper _vietcomBankHelper;
        private readonly IBankStateHelper _bankStateHelper;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;

        public Worker(ILogger<Worker> logger,
            IPayGroupMentService payGroupMentService,
            IRedisService redisService,
            IBankStateHelper bankStateHelper,
            IVietcomBankHelper vietcomBankHelper,
            IPayOrderDepositsMongoService payOrderDepositsMongoService
            )
        {
            _logger = logger;
            _payGroupMentService = payGroupMentService;
            _redisService = redisService;
            _vietcomBankHelper = vietcomBankHelper;
            _bankStateHelper = bankStateHelper;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var isvcb = AppSettings.Configuration["IsVcb"].ToInt();
            var time = AppSettings.Configuration["Time"].ToInt();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                List<BankBalanceModel> bankBalanceModels = new List<BankBalanceModel>();

                var payMents = _redisService.GetPayMents();

                //��ȡ24Сʱ�ڵ��տ��
                var endTime = DateTime.Now.AddHours(1);
                var startTime = endTime.AddHours(-6);
                var payOrderDeposits = await _payOrderDepositsMongoService.GetPayOrderDepositByDateRange(startTime, endTime);
                if (isvcb == 0)
                {
                    _logger.LogInformation("NoVcb������ʼ: {time}", DateTimeOffset.Now);
                    var payorders = payOrderDeposits.GroupBy(r => r.PayMentId);
                    foreach (var item in payorders)
                    {
                        var payMentId = item.Key;
                        var paymentInfo = payMents.FirstOrDefault(r => r.Id == payMentId && r.IsDeleted == false);
                        if (paymentInfo != null && paymentInfo.Type != PayMentTypeEnum.VietcomBank)
                        {
                            var balance = Convert.ToDecimal(0);
                            if (paymentInfo.Type == PayMentTypeEnum.ACBBank)
                            {
                                var orderInfo = item.OrderByDescending(r => Convert.ToInt32(r.RefNo)).FirstOrDefault();
                                if (orderInfo != null)
                                {
                                    balance = orderInfo.AvailableBalance;
                                }
                            }
                            else
                            {
                                var orderInfo = item.OrderByDescending(r => r.TransactionTime).FirstOrDefault();
                                if (orderInfo != null)
                                {
                                    balance = orderInfo.AvailableBalance;
                                }
                            }
                            BankBalanceModel bankBalanceModel = new BankBalanceModel()
                            {
                                PayMentId = paymentInfo.Id,
                                Type = paymentInfo.Type,
                                UserName = paymentInfo.Phone,
                                Balance = balance,
                                UpdateTime = DateTime.Now
                            };
                            bankBalanceModels.Add(bankBalanceModel);
                        }
                    }
                    _logger.LogInformation("NoVcb����������: {time}", DateTimeOffset.Now);

                    //�ж��Ƿ���Ҫ����
                    //var payGroupMentList = _payGroupMentService.GetWhere(r => r.IsDeleted == false && r.Status == true);
                    //foreach (var model in payGroupMentList)
                    //{
                    //    var payment = payMents.FirstOrDefault(r => r.Id == model.PayMentId && r.IsDeleted == false);
                    //    if (payment != null)
                    //    {
                    //        var state = _bankStateHelper.GetPayState(payment.Phone, payment.Type);
                    //        //��ȡ״̬
                    //        var payInfo = payGroupMentList.FirstOrDefault(r => r.Id == model.Id);
                    //        if (payInfo != null)
                    //        {
                    //            payInfo.Status = false;
                    //            _payGroupMentService.Update(payInfo);
                    //            //���»���
                    //            var cacheInfo = _redisService.GetPayGroupMentByPayMentId(model.PayMentId);
                    //            if (cacheInfo != null)
                    //            {
                    //                var info = cacheInfo.PayMents.FirstOrDefault(r => r.Id == model.PayMentId);
                    //                if (info != null)
                    //                {
                    //                    cacheInfo.PayMents.Remove(info);
                    //                    info.UseStatus = false;
                    //                    cacheInfo.PayMents.Add(info);
                    //                    _redisService.AddPayGroupMentRedisValue(cacheInfo.GroupId.ToString(), cacheInfo);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                }

                if (isvcb == 1)
                {
                    _logger.LogInformation("IsVcb������ʼ: {time}", DateTimeOffset.Now);
                    try
                    {
                        var payorders = payOrderDeposits.GroupBy(r => r.PayMentId);
                        foreach (var item in payorders)
                        {
                            var payMentId = item.Key;
                            var paymentInfo = payMents.FirstOrDefault(r => r.Id == payMentId && r.IsDeleted == false);
                            if (paymentInfo != null && paymentInfo.Type == PayMentTypeEnum.VietcomBank)
                            {
                                var balance = Convert.ToDecimal(0);

                                var islogin = IsLogin(paymentInfo.CardNumber);
                                var payGroupMent = await _payGroupMentService.GetFirstAsync(r => r.PayMentId == payMentId);
                                if (payGroupMent != null)
                                {
                                    var isUse = payGroupMent.Status;
                                    if (islogin == 1)
                                    {
                                        var paygroup = _redisService.GetPayGroupMentRedisValue(payGroupMent.GroupId.ToString());
                                        if (paygroup != null)
                                        {
                                            if (!paygroup.VietcomApi.IsNullOrEmpty())
                                            {
                                                balance = await _vietcomBankHelper.GetBalance(paymentInfo.Phone, paymentInfo.CardNumber, paygroup.VietcomApi);
                                            }
                                        }

                                        BankBalanceModel bankBalanceModel = new BankBalanceModel()
                                        {
                                            PayMentId = paymentInfo.Id,
                                            Type = paymentInfo.Type,
                                            UserName = paymentInfo.Phone,
                                            Balance = balance,
                                            UpdateTime = DateTime.Now
                                        };
                                        bankBalanceModels.Add(bankBalanceModel);
                                    }
                                }
                            }
                        }
                        _logger.LogInformation("IsVcb����������: {time}", DateTimeOffset.Now);
                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error("Vcb�������쳣��" + ex.ToString());
                    }
                }

                //����
                foreach (var model in bankBalanceModels)
                {
                    var checkBalance = _redisService.GetBalanceByPaymentId(model.PayMentId);
                    if (checkBalance != null)
                    {
                        checkBalance.Balance = model.Balance;
                        checkBalance.UpdateTime = DateTime.Now;
                        _redisService.SetBalance(model.PayMentId, checkBalance);
                    }
                    else
                    {
                        var balance = new BankBalanceModel()
                        {
                            PayMentId = model.PayMentId,
                            Type = model.Type,
                            UserName = model.UserName,
                            Balance = model.Balance,
                            Balance2 = model.Balance,
                            UpdateTime = DateTime.Now
                        };
                        _redisService.SetBalance(model.PayMentId, balance);
                    }
                }

                await Task.Delay(time, stoppingToken);
            }
        }
        public int IsLogin(string account)
        {
            var cacheToken = _redisService.GetRedisValue<VietcomBankLoginResponse>(CommonHelper.GetBankCacheBankKey(PayMentTypeEnum.VietcomBank, account));
            if (cacheToken == null)
            {
                return 0;
            }
            if (cacheToken.userInfo == null)
            {
                return 0;
            }
            if (cacheToken.userInfo.mobileId.IsNullOrEmpty())
            {
                return 0;
            }
            return 1;
        }
    }
}
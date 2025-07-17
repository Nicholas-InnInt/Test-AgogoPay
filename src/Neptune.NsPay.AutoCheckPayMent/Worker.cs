using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;

namespace Neptune.NsPay.AutoCheckPayMent
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPayOrdersMongoService _payOrdersMongoService;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IRedisService _redisService;
        private readonly IBankBalanceService _bankBalanceService;
        private readonly IBankStateHelper _bankStateHelper;

        public Worker(ILogger<Worker> logger,
            IPayOrdersMongoService payOrdersMongoService,
            IPayGroupMentService payGroupMentService,
            IRedisService redisService,
            IBankBalanceService bankBalanceService,
            IBankStateHelper bankStateHelper)
        {
            _logger = logger;
            _payGroupMentService = payGroupMentService;
            _redisService = redisService;
            _payOrdersMongoService = payOrdersMongoService;
            _bankBalanceService = bankBalanceService;
            _bankStateHelper = bankStateHelper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                var payMents = _redisService.GetPayMents();
                foreach (var item in payMents)
                {
                    try
                    {
                        if (item.BalanceLimitMoney > 0)
                        {
                            //�ж�����Ƿ񳬹��޶��Ƿ���Ҫ����
                            var islogin = _bankStateHelper.GetPayState(item.Phone, item.Type);
                            var payMentBalance = _redisService.GetBalance(item.Id, item.Type);
                            var balance = await _bankBalanceService.GetBalance(item.Id, payMentBalance.Balance, payMentBalance.Balance2, islogin);
                            if (balance > item.BalanceLimitMoney)
                            {
                                //����״̬
                                var paygroupment = await _payGroupMentService.GetFirstAsync(r => r.PayMentId == item.Id && r.IsDeleted == false);
                                if (paygroupment != null)
                                {
                                    paygroupment.Status = false;
                                    await _payGroupMentService.UpdateAsync(paygroupment);
                                    //���»���
                                    var cacheInfo = _redisService.GetPayGroupMentByPayMentId(item.Id);
                                    if (cacheInfo != null)
                                    {
                                        var info = cacheInfo.PayMents.FirstOrDefault(r => r.Id == item.Id);
                                        if (info != null)
                                        {
                                            _logger.LogInformation("PayMent:" + item.CardNumber + "�Ƴ�");
                                            cacheInfo.PayMents.Remove(info);
                                            info.UseStatus = false;
                                            cacheInfo.PayMents.Add(info);
                                            _redisService.AddPayGroupMentRedisValue(cacheInfo.GroupId.ToString(), cacheInfo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NlogLogger.Error("����쳣��" + ex.ToString());
                    }

                    if (item.IsDeleted == false)
                    {
                        switch (item.Type)
                        {
                            case PayMentTypeEnum.MBBank:
                            case PayMentTypeEnum.ACBBank:
                            case PayMentTypeEnum.BusinessMbBank:
                            case PayMentTypeEnum.BusinessTcbBank:
                            case PayMentTypeEnum.BusinessVtbBank:
                            case PayMentTypeEnum.MsbBank:
                            case PayMentTypeEnum.SeaBank:
                            case PayMentTypeEnum.BvBank:
                            case PayMentTypeEnum.NamaBank:
                            case PayMentTypeEnum.TPBank:
                            case PayMentTypeEnum.VPBBank:
                            case PayMentTypeEnum.OCBBank:
                            case PayMentTypeEnum.EXIMBank:
                            case PayMentTypeEnum.NCBBank:
                            case PayMentTypeEnum.HDBank:
                            case PayMentTypeEnum.LPBank:
                            case PayMentTypeEnum.PGBank:
                            case PayMentTypeEnum.VietBank:
                            case PayMentTypeEnum.BacaBank:
                                //�ж��Ƿ����ߣ����߹ر�
                                var state = _bankStateHelper.GetPayState(item.CardNumber, item.Type);
                                if (state == 0)
                                {
                                    //����״̬
                                    var paygroupment = await _payGroupMentService.GetFirstAsync(r => r.PayMentId == item.Id && r.Status == true && r.IsDeleted == false);
                                    if (paygroupment != null)
                                    {
                                        paygroupment.Status = false;
                                        await _payGroupMentService.UpdateAsync(paygroupment);
                                        //���»���
                                        var cacheInfo = _redisService.GetPayGroupMentByPayMentId(item.Id);
                                        if (cacheInfo != null)
                                        {
                                            var info = cacheInfo.PayMents.FirstOrDefault(r => r.Id == item.Id);
                                            if (info != null)
                                            {
                                                _logger.LogInformation("PayMent:" + item.CardNumber + "δ��¼���Ƴ�");
                                                cacheInfo.PayMents.Remove(info);
                                                info.UseStatus = false;
                                                cacheInfo.PayMents.Add(info);
                                                _redisService.AddPayGroupMentRedisValue(cacheInfo.GroupId.ToString(), cacheInfo);
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                await Task.Delay(1000 * 10, stoppingToken);
            }
        }
    }
}
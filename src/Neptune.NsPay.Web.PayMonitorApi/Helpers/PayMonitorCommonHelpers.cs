using AutoMapper;
using Neptune.NsPay.Commons;
using Neptune.NsPay.HttpExtensions.Bank.Helpers.Interfaces;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayGroupMents.Dtos;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.RedisExtensions;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.SignalRClient;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using Neptune.NsPay.Web.PayMonitorApi.Models;
using Neptune.NsPay.Web.PayMonitorApi.Models.SignalR;
using Neptune.NsPay.Web.PayMonitorApi.Service;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;

namespace Neptune.NsPay.Web.PayMonitorApi.Helpers
{
    public class PayMonitorCommonHelpers : IPayMonitorCommonHelpers
    {
        private readonly static string NsPayAll = "NsPayAll";

        private readonly IRedisService _redisService;
        private readonly IBankStateHelper _bankStateHelper;
        private readonly IBankBalanceService _bankBalanceService;
        private readonly IPushUpdateService _pushUpdateService;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IMapper _mapper;

        public PayMonitorCommonHelpers(
            IRedisService redisService,
            IBankStateHelper bankStateHelper,
            IBankBalanceService bankBalanceService,
            IPushUpdateService pushUpdateService,
            IPayGroupMentService payGroupMentService)
        {

            _redisService = redisService;
            _bankStateHelper = bankStateHelper;
            _bankBalanceService = bankBalanceService;
            _pushUpdateService = pushUpdateService;
            _payGroupMentService = payGroupMentService;
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = new Mapper(configuration);

        }

        public IMapper Mapper()
        {
            return _mapper;
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<MerchantDto, ListenMerchantData>();
                CreateMap<PayMentDto, ListenPaymentData>();
                CreateMap<PayGroupMentDto, ListenPayGroupMentData>();
            }
        }
        public int GetLimitStatus(decimal balanceLimitMoney, decimal balance)
        {
            int status = 0;
            if (balanceLimitMoney > 0)
            {
                if (balance > balanceLimitMoney)
                {
                    status = 1;
                }
            }
            return status;
        }

        public DateTime? ConvertToStandardTime(string dateStr, string? dateTimeZoneGMT = null)  // if dateTimeZoneGMT null meaning pass in date str inside got timezone included
        {
            string standardTimeFormat = "yyyy/MM/dd HH:mm:ss zzz";
            DateTime? returnDateTime = null;

            if (DateTime.TryParseExact(dateStr + (string.IsNullOrEmpty(dateTimeZoneGMT) ? string.Empty : (" " + dateTimeZoneGMT)), standardTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var _convertedDateTime))
            {
                returnDateTime = _convertedDateTime;
            }
            return returnDateTime;
        }

        private List<MerchantPaymentItem> ContructSignalRDetails(List<PayMentRedisModel> paymentList)
        {

            List<MerchantPaymentItem> returnModel = new List<MerchantPaymentItem>();
            foreach (var item in paymentList)
            {
                var payMentBalance = _redisService.GetBalance(item.Id, item.Type);

                MerchantPaymentItem pay = new MerchantPaymentItem();

                pay.Id = item.Id;
                pay.PayType = item.Type.ToString();
                pay.PayName = item.Name;
                pay.Name = item.FullName;
                pay.Phone = item.Phone;
                pay.BusinessNo = item.Mail;
                pay.PassWord = item.PassWord;
                pay.CardNo = item.CardNumber;
                pay.State = _bankStateHelper.GetPayState(item.CardNumber, item.Type);
                pay.Account = _bankStateHelper.GetAccount(item.CardNumber, item.Type);
                pay.Balance = item.Type switch
                {
                    PayMentTypeEnum.VietcomBank or
                    PayMentTypeEnum.VietinBank or
                    PayMentTypeEnum.BidvBank or
                    PayMentTypeEnum.TechcomBank or
                    PayMentTypeEnum.BusinessTcbBank or
                    PayMentTypeEnum.MsbBank or
                    PayMentTypeEnum.SeaBank or
                    PayMentTypeEnum.BvBank or
                    PayMentTypeEnum.TPBank or
                    PayMentTypeEnum.VPBBank or
                    PayMentTypeEnum.OCBBank or
                    PayMentTypeEnum.EXIMBank or
                    PayMentTypeEnum.NCBBank or
                    PayMentTypeEnum.HDBank or
                    PayMentTypeEnum.LPBank or
                    PayMentTypeEnum.PGBank or
                    PayMentTypeEnum.VietBank or
                    PayMentTypeEnum.BacaBank
                      => payMentBalance.Balance2,
                    _ => _bankBalanceService.GetBalance(item.Id, payMentBalance.Balance, payMentBalance.Balance2, pay.State).Result,
                };
                pay.Status = item.UseStatus ? 1 : 0;
                pay.LimitStatus = GetLimitStatus(item.BalanceLimitMoney, pay.Balance);
                pay.IsUse = _redisService.GetPayUseMent(item.Id) is not null and > 0;

                returnModel.Add(pay);
            }

            return returnModel;
        }

        public async Task<bool> NotifyMerchantPaymentChanged(int PaymentId)
        {
            Stopwatch perfSW = Stopwatch.StartNew();
            Dictionary<string, List<MerchantPaymentItem>> merchantPaymentDict = new Dictionary<string, List<MerchantPaymentItem>>();
            try
            {
                var onlineMerchant = NotificationHub.GetOnlineMerchantCode();

                if (onlineMerchant.Count > 0)
                {
                    var allPaymentList = _redisService.GetPayMents().Where(r => r.Type != PayMentTypeEnum.NoPay && r.Type != PayMentTypeEnum.ScratchCards && r.Type != PayMentTypeEnum.MoMoPay && r.Type != PayMentTypeEnum.ZaloPay && r.DispenseType == PayMentDispensEnum.None && r.ShowStatus == PayMentStatusEnum.Show).ToList();
                    if (onlineMerchant.Contains(NsPayAll))
                    {
                        var convertedModel = ContructSignalRDetails(allPaymentList);
                        // Add Payment Method Will Included for this merchant
                        merchantPaymentDict.Add(NsPayAll, convertedModel);

                    }

                    var affectedPayGroup = _redisService.GetPayGroupMentIdByPayMentId(PaymentId);
                    var affectedMerchant = _redisService.GetMerchantRedis().Where(x => affectedPayGroup.Contains(x.PayGroupId.ToString()) && onlineMerchant.Contains(x.MerchantCode));

                    if (affectedMerchant.Any(x => x.MerchantType == Merchants.MerchantTypeEnum.External && affectedPayGroup.Contains(x.PayGroupId.ToString())) || (onlineMerchant.Contains(NsPayRedisKeyConst.NsPay)))
                    {
                        var groupInfo = _redisService.GetPayGroupMentByGroupName(_redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.NsPayGroupName));

                        if (groupInfo != null)
                        {
                            merchantPaymentDict.Add(NsPayRedisKeyConst.NsPay, ContructSignalRDetails(groupInfo.PayMents.Join(allPaymentList, t1 => t1.Id, t2 => t2.Id, (t1, t2) => t1).ToList()));
                        }
                    }

                    foreach (var merchant in affectedMerchant)
                    {

                        if (!merchantPaymentDict.ContainsKey(merchant.MerchantCode))
                        {
                            var groupInfoMerchant = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString());

                            if (groupInfoMerchant != null)
                            {
                                merchantPaymentDict.Add(merchant.MerchantCode, ContructSignalRDetails(groupInfoMerchant.PayMents.Join(allPaymentList, t1 => t1.Id, t2 => t2.Id, (t1, t2) => t1).ToList()));
                            }
                        }

                    }

                    var timeStampUnix = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                    foreach (var merchantPayment in merchantPaymentDict)
                    {
                        await _pushUpdateService.MerchantPaymentChanged(new MerchantPaymentDetails() { MerchantCode = merchantPayment.Key, CreateUnixTimestamp = timeStampUnix, Data = merchantPayment.Value });
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("NotifyMerchantPaymentChanged - error ", ex);
            }
            finally
            {
                perfSW.Stop();
                NlogLogger.Info("NotifyMerchantPaymentChanged - " + JsonConvert.SerializeObject(merchantPaymentDict.Keys) + " taken " + perfSW.ElapsedMilliseconds + "ms");
            }

            return true;

        }

        public async Task<bool> InitialDataMerchant(string MerchantCode)
        {

            bool isSuccess = false;

            try
            {
                if (MerchantCode != NsPayAll)
                {
                    var targetMerchant = _redisService.GetMerchantRedis().FirstOrDefault(x => x.MerchantCode == MerchantCode);

                    if (targetMerchant != null)
                    {
                        await NotifyMerchantChanged(targetMerchant.Id);
                    }
                }

                isSuccess = true;

            }
            catch (Exception ex)
            {
                NlogLogger.Error(ex.Message);
            }

            return isSuccess;
        }

        public async Task<bool> NotifyAllMerchant()
        {
            bool isSuccess = false;
            Dictionary<string, List<MerchantPaymentItem>> merchantPaymentDict = new Dictionary<string, List<MerchantPaymentItem>>();
            Stopwatch perfSW =  Stopwatch.StartNew();
            try
            {
                var onlineMerchant = NotificationHub.GetOnlineMerchantCode();

                if(onlineMerchant.Count>0)
                {
                    var targetMerchant = _redisService.GetMerchantRedis().Where(x => onlineMerchant.Contains(x.MerchantCode));
                    var allPaymentList = _redisService.GetPayMents().Where(r => r.Type != PayMentTypeEnum.NoPay && r.Type != PayMentTypeEnum.ScratchCards && r.Type != PayMentTypeEnum.MoMoPay && r.Type != PayMentTypeEnum.ZaloPay && r.DispenseType == PayMentDispensEnum.None && r.ShowStatus == PayMentStatusEnum.Show).ToList();

                    if (onlineMerchant.Contains(NsPayAll))
                    {
                        var convertedModel = ContructSignalRDetails(allPaymentList);
                        // Add Payment Method Will Included for this merchant
                        merchantPaymentDict.Add(NsPayAll, convertedModel);
                    }


                    if(onlineMerchant.Contains(NsPayRedisKeyConst.NsPay))
                    {
                        var groupInfo = _redisService.GetPayGroupMentByGroupName(_redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.NsPayGroupName));

                        if (groupInfo != null)
                        {
                            merchantPaymentDict.Add(NsPayRedisKeyConst.NsPay, ContructSignalRDetails(groupInfo.PayMents.Join(allPaymentList, t1 => t1.Id, t2 => t2.Id, (t1, t2) => t1).ToList()));
                        }
                    }

                    if (targetMerchant != null && targetMerchant.Count() > 0    )
                    {

                        if (targetMerchant != null)
                        {
                            foreach (var merchant in targetMerchant)
                            {
                                if (!merchantPaymentDict.ContainsKey(merchant.MerchantCode))
                                {
                                    var groupInfoMerchant = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString());

                                    if (groupInfoMerchant != null)
                                    {
                                        merchantPaymentDict.Add(merchant.MerchantCode, ContructSignalRDetails(groupInfoMerchant.PayMents.Join(allPaymentList, t1 => t1.Id, t2 => t2.Id, (t1, t2) => t1).ToList()));
                                    }
                                }

                            }
                        }

                        var timeStampUnix = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                        foreach (var merchantPayment in merchantPaymentDict)
                        {
                            await _pushUpdateService.MerchantPaymentChanged(new MerchantPaymentDetails() { MerchantCode = merchantPayment.Key, CreateUnixTimestamp = timeStampUnix, Data = merchantPayment.Value });
                        }
                    }
                }

                isSuccess = true;
            }
            catch (Exception ex)
            {
                NlogLogger.Error(ex.Message);
            }
            finally
            {
                perfSW.Stop();
                NlogLogger.Info("NotifyAllMerchant - " + JsonConvert.SerializeObject(merchantPaymentDict.Keys) + " taken " + perfSW.ElapsedMilliseconds + "ms");
            }

            return isSuccess;
        }

        public async Task<bool> NotifyMerchantChanged(int MerchantId)
        {
            var isSuccess = false;
            try
            {
                var currentMerchant = _redisService.GetMerchantRedis().FirstOrDefault(x => x.Id == MerchantId);
                Dictionary<string, List<MerchantPaymentItem>> merchantPaymentDict = new Dictionary<string, List<MerchantPaymentItem>>();
                if (currentMerchant != null)
                {
                    var allPaymentList = _redisService.GetPayMents().Where(r => r.Type != PayMentTypeEnum.NoPay && r.Type != PayMentTypeEnum.ScratchCards && r.Type != PayMentTypeEnum.MoMoPay && r.Type != PayMentTypeEnum.ZaloPay && r.DispenseType == PayMentDispensEnum.None && r.ShowStatus == PayMentStatusEnum.Show).ToList();
                    var groupInfoMerchant = _redisService.GetPayGroupMentRedisValue(currentMerchant.PayGroupId.ToString());

                    if (groupInfoMerchant != null)
                    {
                        merchantPaymentDict.Add(currentMerchant.MerchantCode, ContructSignalRDetails(groupInfoMerchant.PayMents.Join(allPaymentList, t1 => t1.Id, t2 => t2.Id, (t1, t2) => t1).ToList()));
                    }
                }

                var timeStampUnix = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                foreach (var merchantPayment in merchantPaymentDict)
                {
                    await _pushUpdateService.MerchantPaymentChanged(new MerchantPaymentDetails() { MerchantCode = merchantPayment.Key, CreateUnixTimestamp = timeStampUnix, Data = merchantPayment.Value });
                }
                isSuccess = true;

            }
            catch (Exception ex)
            {
                NlogLogger.Error(ex.Message);
            }


            return isSuccess;
        }

        public async Task<bool> NotifyPayGroupChanged(int PayGroupId)
        {
            var isSuccess = false;
            Stopwatch perfSW = Stopwatch.StartNew();
            Dictionary<string, List<MerchantPaymentItem>> merchantPaymentDict = new Dictionary<string, List<MerchantPaymentItem>>();
            try
            {
                var onlineMerchant = NotificationHub.GetOnlineMerchantCode();

                if(onlineMerchant.Count > 0)
                {
                    var allPaymentList = _redisService.GetPayMents().Where(r => r.Type != PayMentTypeEnum.NoPay && r.Type != PayMentTypeEnum.ScratchCards && r.Type != PayMentTypeEnum.MoMoPay && r.Type != PayMentTypeEnum.ZaloPay && r.DispenseType == PayMentDispensEnum.None && r.ShowStatus == PayMentStatusEnum.Show).ToList();
                   
                    if(onlineMerchant.Contains(NsPayAll))
                    {
                        var convertedModel = ContructSignalRDetails(allPaymentList);
                        // Add Payment Method Will Included for this merchant
                        merchantPaymentDict.Add(NsPayAll, convertedModel);
                    }
                    
                   // var affectedPayGroup = _redisService.GetPayGroupMentRedisValue(PayGroupId.ToString());
                    var affectedMerchant = (_redisService.GetMerchantRedis()?.Where(x => x.PayGroupId.ToString() == PayGroupId.ToString() && onlineMerchant.Contains(x.MerchantCode))??new List<MerchantRedisModel>() );

                    if (affectedMerchant.Any(x => x.MerchantType == Merchants.MerchantTypeEnum.External) || (onlineMerchant.Contains(NsPayRedisKeyConst.NsPay)))
                    {
                        var groupInfo = _redisService.GetPayGroupMentByGroupName(_redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.NsPayGroupName));

                        if (groupInfo != null)
                        {
                            merchantPaymentDict.Add(NsPayRedisKeyConst.NsPay, ContructSignalRDetails(groupInfo.PayMents.Join(allPaymentList, t1 => t1.Id, t2 => t2.Id, (t1, t2) => t1).ToList()));
                        }
                    }

                    foreach (var merchant in affectedMerchant)
                    {

                        if (!merchantPaymentDict.ContainsKey(merchant.MerchantCode))
                        {
                            var groupInfoMerchant = _redisService.GetPayGroupMentRedisValue(merchant.PayGroupId.ToString());

                            if (groupInfoMerchant != null)
                            {
                                merchantPaymentDict.Add(merchant.MerchantCode, ContructSignalRDetails(groupInfoMerchant.PayMents.Join(allPaymentList, t1 => t1.Id, t2 => t2.Id, (t1, t2) => t1).ToList()));
                            }
                        }

                    }

                    var timeStampUnix = TimeHelper.GetUnixTimeStamp(DateTime.Now);
                    foreach (var merchantPayment in merchantPaymentDict)
                    {
                        await _pushUpdateService.MerchantPaymentChanged(new MerchantPaymentDetails() { MerchantCode = merchantPayment.Key, CreateUnixTimestamp = timeStampUnix, Data = merchantPayment.Value });
                    }
                }

                isSuccess = true;
            }
            catch (Exception ex)
            {
                NlogLogger.Error(ex.Message);
            }
            finally
            {
                perfSW.Stop();
                NlogLogger.Info("NotifyPayGroupChanged - " + JsonConvert.SerializeObject(merchantPaymentDict.Keys) + " taken " + perfSW.ElapsedMilliseconds + "ms");
            }

            return isSuccess;
        }

        public async Task<bool> DetermineChangesAndNotify(MerchantPaymentChangedDto changes)
        {
            bool isSuccess = false;

            try
            {
                List<int> affectedMerchant = new List<int>();
                List<int> affectedPayment = new List<int>();
                List<int> affectedPayGroupMent = new List<int>();

                NlogLogger.Info("DetermineChangesAndNotify ：" + JsonConvert.SerializeObject(changes));

                if (changes.Merchant != null)
                {
                    foreach (var merchant in changes.Merchant)
                    {

                        if (merchant.OldData == null || merchant.NewData == null || (!_mapper.Map<ListenMerchantData>(merchant.OldData).Equals(_mapper.Map<ListenMerchantData>(merchant.NewData))))
                        {
                            affectedMerchant.Add((merchant.NewData ?? merchant.OldData).Id);
                        }

                    }
                }

                if (changes.Payment != null)
                {
                    foreach (var payment in changes.Payment)
                    {

                        if (payment.OldData == null || payment.NewData == null || (!_mapper.Map<ListenPaymentData>(payment.OldData).Equals(_mapper.Map<ListenPaymentData>(payment.NewData))))
                        {
                            affectedPayment.Add((payment.NewData ?? payment.OldData).Id);
                        }

                    }
                }

                if (changes.PayGroupMent != null)
                {
                    foreach (var payGroupMent in changes.PayGroupMent)
                    {

                        if (payGroupMent.OldData == null || payGroupMent.NewData == null || (!_mapper.Map<ListenPayGroupMentData>(payGroupMent.OldData).Equals(_mapper.Map<ListenPayGroupMentData>(payGroupMent.NewData))))
                        {
                            affectedPayGroupMent.Add((payGroupMent.NewData ?? payGroupMent.OldData).GroupId);
                        }

                    }
                }

                foreach (var merchant in affectedMerchant.Distinct())
                {
                    await NotifyMerchantChanged(merchant);
                }

                foreach (var payment in affectedPayment.Distinct())
                {
                    await NotifyMerchantPaymentChanged(payment);
                }

                foreach (var payGroupMent in affectedPayGroupMent.Distinct())
                {
                    await NotifyPayGroupChanged(payGroupMent);
                }

                isSuccess = true;
            }
            catch (Exception ex)
            {
                NlogLogger.Error(ex.Message);
            }

            return isSuccess;
        }

        public async Task<bool> UpdatePaymentUseState(int PaymentId, decimal NewBalance)
        {
            bool haveUpdate = false;

            try
            {
                var payment = _redisService.GetPayMents().FirstOrDefault(r => r.Id == PaymentId);

                if (payment != null && payment.BalanceLimitMoney > 0)
                {
                    if (NewBalance > payment.BalanceLimitMoney && payment.UseStatus)
                    {
                        //����״̬
                        var paygroupment = _payGroupMentService.GetFirst(r => r.PayMentId == payment.Id && r.IsDeleted == false);
                        if (paygroupment != null && paygroupment.Status)
                        {
                            paygroupment.Status = false;
                            _payGroupMentService.Update(paygroupment);
                            //���»���
                            var cacheInfo = _redisService.GetPayGroupMentByPayMentId(payment.Id);
                            if (cacheInfo != null)
                            {
                                var info = cacheInfo.PayMents.FirstOrDefault(r => r.Id == payment.Id);
                                if (info != null)
                                {
                                    cacheInfo.PayMents.Remove(info);
                                    info.UseStatus = false;
                                    cacheInfo.PayMents.Add(info);
                                    _redisService.AddPayGroupMentRedisValue(cacheInfo.GroupId.ToString(), cacheInfo);
                                }
                            }

                            haveUpdate = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error(ex.Message);
            }

            return haveUpdate;

        }
    }
}
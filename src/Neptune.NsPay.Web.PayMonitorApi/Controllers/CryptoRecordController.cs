using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Commons;
using Neptune.NsPay.Commons.Models;
using Neptune.NsPay.HttpExtensions.Crypto.Dtos;
using Neptune.NsPay.HttpExtensions.Crypto.Enums;
using Neptune.NsPay.HttpExtensions.Crypto.Helpers.Interfaces;
using Neptune.NsPay.KafkaExtensions;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.SqlSugarExtensions.Services;
using Neptune.NsPay.SqlSugarExtensions.Services.Interfaces;
using SqlSugar;
using System.Collections.Concurrent;

namespace Neptune.NsPay.Web.PayMonitorApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CryptoRecordController : ControllerBase
    {
        private readonly ITronCryptoHelper _tronCryptoHelper;
        private readonly IPayMentService _payMentService;
        private readonly IPayOrderDepositsMongoService _payOrderDepositsMongoService;
        private readonly IMerchantService _merchantService;
        private readonly IPayGroupMentService _payGroupMentService;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IEthereumCryptoHelper _ethereumCryptoHelper;

        public CryptoRecordController(
            ITronCryptoHelper tronCryptoHelper,
            IPayMentService payMentService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            IMerchantService merchantService,
            IPayGroupMentService payGroupMentService,
            IKafkaProducer kafkaProducer,
            IEthereumCryptoHelper ethereumCryptoHelper)
        {
            _tronCryptoHelper = tronCryptoHelper;
            _payMentService = payMentService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _merchantService = merchantService;
            _payGroupMentService = payGroupMentService;
            _kafkaProducer = kafkaProducer;
            _ethereumCryptoHelper = ethereumCryptoHelper;
        }

        [HttpPost]
        public async Task<IActionResult> Record([FromBody] int paymentType)
        {
            var paymentTypeEnum = (PayMentTypeEnum)paymentType;
            if (paymentTypeEnum != PayMentTypeEnum.USDT_TRC20 && paymentTypeEnum != PayMentTypeEnum.USDT_ERC20)
                return BadRequest($"Unsupported payment type: {paymentType}");

            var paymentList = _payMentService.GetWhere(x => x.Type == paymentTypeEnum && x.Status == PayMentStatusEnum.Show && !x.IsDeleted);
            if (paymentList is not { Count: > 0 })
                return Ok("No Tron USDT payment found");

            var payGroupMent = _payGroupMentService.GetWhere(x => paymentList.Select(y => y.Id).Contains(x.PayMentId) && x.Status && !x.IsDeleted);
            if (payGroupMent is not { Count: > 0 })
                return Ok("No payment methods found for this merchant");

            var merchantList = _merchantService.GetWhere(x => payGroupMent.Select(y => y.GroupId).Contains(x.PayGroupId) && !x.IsDeleted);
            if (merchantList is not { Count: > 0 })
                return Ok($"No merchant not found");

            var merchantPaymentList = (from pgm in payGroupMent
                                       join pm in paymentList on pgm.PayMentId equals pm.Id
                                       join mc in merchantList on pgm.GroupId equals mc.PayGroupId
                                       group new { pm, mc } by new { mc.Id, mc.MerchantCode } into g
                                       select new
                                       {
                                           Id = g.Key.Id,
                                           Code = g.Key.MerchantCode,
                                           Payments = g.Select(x => new
                                           {
                                               x.pm.Id,
                                               x.pm.CryptoWalletAddress
                                           }).ToList()
                                       }).ToList();

            var cryptoWalletTransactionDict = new ConcurrentDictionary<string, List<TokenTransactionDto>>();
            await Parallel.ForEachAsync(merchantPaymentList.SelectMany(x => x.Payments.Select(y => y.CryptoWalletAddress)).ToHashSet(), async (cryptoWalletAddress, ct) =>
            {
                try
                {
                    var transactions = paymentTypeEnum switch
                    {
                        PayMentTypeEnum.USDT_TRC20 => await _tronCryptoHelper.GetTokenTransactionsByAddress(TronTokenTypeEnum.USDT, cryptoWalletAddress),
                        PayMentTypeEnum.USDT_ERC20 => await _ethereumCryptoHelper.GetTokenTransactionsByAddress(TronTokenTypeEnum.USDT, cryptoWalletAddress),
                        _ => throw new NotSupportedException($"Unsupported payment type: {paymentType}")
                    };

                    cryptoWalletTransactionDict[cryptoWalletAddress] = transactions;
                }
                catch (Exception ex)
                {
                    NlogLogger.Error($"Error processing CryptoWalletAddress {cryptoWalletAddress}: {ex.Message}", ex);
                }
            });

            var merchantNewPayOrderListDict = new ConcurrentDictionary<string, List<string>>();
            await Parallel.ForEachAsync(merchantPaymentList, async (merchant, ct) =>
            {
                try
                {
                    var newPayOrderDepositIdList = new List<string>();

                    foreach (var payment in merchant.Payments)
                    {
                        try
                        {
                            //var transactions = cryptoWalletTransactionDict[payment.CryptoWalletAddress];

                            if (cryptoWalletTransactionDict.ContainsKey(payment.CryptoWalletAddress) && cryptoWalletTransactionDict.GetValueOrDefault(payment.CryptoWalletAddress) is { Count: > 0 } transactions)
                            {
                                foreach (var transaction in transactions)
                                {
                                    var existingPayOrderDeposit = await _payOrderDepositsMongoService.GetPayOrderByBankNoDesc(transaction.Hash, payment.Id);
                                    if (existingPayOrderDeposit is not null) continue;

                                    var payOrderDeposit = new PayOrderDepositsMongoEntity
                                    {
                                        PayType = paymentType.ToInt(),
                                        UserName = "",
                                        AccountNo = payment.CryptoWalletAddress,
                                        RefNo = transaction.Hash,
                                        Description = "",
                                        AvailableBalance = 0,
                                        PayMentId = payment.Id,
                                        MerchantCode = merchant.Code,
                                        MerchantId = merchant.Id,
                                        TransactionTime = transaction.Timestamp,
                                    };

                                    // Credit
                                    if (transaction.To == payment.CryptoWalletAddress)
                                    {
                                        payOrderDeposit.CreditBank = transaction.TokenName;
                                        payOrderDeposit.CreditAcctNo = transaction.From;
                                        payOrderDeposit.CreditAcctName = "";
                                        payOrderDeposit.CreditAmount = transaction.Amount;
                                        payOrderDeposit.Type = "CRDT";

                                        var newPayOrderDepositId = await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                        newPayOrderDepositIdList.Add(newPayOrderDepositId);
                                    }
                                    // Debit
                                    else if (transaction.From == payment.CryptoWalletAddress)
                                    {
                                        payOrderDeposit.DebitBank = transaction.TokenName;
                                        payOrderDeposit.DebitAcctNo = transaction.To;
                                        payOrderDeposit.DebitAcctName = "";
                                        payOrderDeposit.DebitAmount = transaction.Amount;
                                        payOrderDeposit.Type = "DBIT";

                                        var newPayOrderDepositId = await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error($"{paymentType} 运行错误: {ex.Message}", ex);
                        }
                    }

                    merchantNewPayOrderListDict[merchant.Code] = newPayOrderDepositIdList;
                }
                catch (Exception ex)
                {
                    NlogLogger.Error($"Error processing merchant {merchant.Code}: {ex.Message}", ex);
                }
            });

            foreach (var (key, value) in merchantNewPayOrderListDict)
            {
                if (value is { Count: > 0 })
                {
                    await _kafkaProducer.ProduceAsync(KafkaTopics.PayOrderCrypto, key, new PayOrderCryptoPublishDto
                    {
                        MerchantCode = key,
                        PaymentType = paymentTypeEnum,
                        PayOrderDepositIds = value,
                    });
                }
            }

            return Ok(merchantNewPayOrderListDict);
        }
    }
}
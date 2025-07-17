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
using Newtonsoft.Json;
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

        public CryptoRecordController(
            ITronCryptoHelper tronCryptoHelper,
            IPayMentService payMentService,
            IPayOrderDepositsMongoService payOrderDepositsMongoService,
            IMerchantService merchantService,
            IPayGroupMentService payGroupMentService,
            IKafkaProducer kafkaProducer)
        {
            _tronCryptoHelper = tronCryptoHelper;
            _payMentService = payMentService;
            _payOrderDepositsMongoService = payOrderDepositsMongoService;
            _merchantService = merchantService;
            _payGroupMentService = payGroupMentService;
            _kafkaProducer = kafkaProducer;
        }

        [HttpPost]
        public async Task<IActionResult> TronUSDTRecord()
        {
            var paymentList = _payMentService.GetWhere(x => x.Type == PayMentTypeEnum.USDT_TRC20);
            if (paymentList is not { Count: > 0 })
                return Ok("No Tron USDT payment found");

            var payGroupMent = _payGroupMentService.GetWhere(x => paymentList.Select(y => y.Id).Contains(x.PayMentId))?.GroupBy(x => x.GroupId).ToDictionary(x => x.Key, x => x.ToList());
            if (payGroupMent is not { Count: > 0 })
                return Ok("No payment methods found for this merchant");

            var merchantList = _merchantService.GetWhere(x => payGroupMent.Keys.Contains(x.PayGroupId) && !x.IsDeleted);
            if (merchantList is not { Count: > 0 })
                return NotFound($"No merchant not found");

            var cryptoWalletTransactionDict = new ConcurrentDictionary<string, List<TronTokenTransactionDto>>();
            await Parallel.ForEachAsync(paymentList.Select(x => x.CryptoWalletAddress).ToHashSet(), async (cryptoWalletAddress, ct) =>
            {
                try
                {
                    var transactions = await _tronCryptoHelper.GetTokenTransactionsByAddress(TronTokenTypeEnum.USDT, cryptoWalletAddress);

                    cryptoWalletTransactionDict[cryptoWalletAddress] = transactions;
                }
                catch (Exception ex)
                {
                    NlogLogger.Error($"Error processing CryptoWalletAddress {cryptoWalletAddress}: {ex.Message}", ex);
                }
            });

            var merchantNewPayOrderListDict = new ConcurrentDictionary<string, List<string>>();
            await Parallel.ForEachAsync(merchantList, async (merchant, ct) =>
            {
                try
                {
                    var newPayOrderDepositIdList = new List<string>();

                    foreach (var payment in paymentList.Where(x => payGroupMent[merchant.PayGroupId].Select(y => y.PayMentId).Contains(x.Id)))
                    {
                        try
                        {
                            var transactions = cryptoWalletTransactionDict[payment.CryptoWalletAddress];

                            if (transactions is { Count: > 0 })
                            {
                                foreach (var transaction in transactions)
                                {
                                    var existingPayOrderDeposit = await _payOrderDepositsMongoService.GetPayOrderByBankNoDesc(transaction.Hash, payment.Id);
                                    if (existingPayOrderDeposit is not null) continue;

                                    var payOrderDeposit = new PayOrderDepositsMongoEntity
                                    {
                                        PayType = PayMentTypeEnum.USDT_TRC20.ToInt(),
                                        UserName = "",
                                        AccountNo = payment.CryptoWalletAddress,
                                        RefNo = transaction.Hash,
                                        Description = "",
                                        AvailableBalance = 0,
                                        PayMentId = payment.Id,
                                        MerchantCode = merchant.MerchantCode,
                                        MerchantId = merchant.Id,
                                        TransactionTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(transaction.BlockTimestamp / 1000d)).ToLocalTime(),
                                    };

                                    // Credit
                                    if (transaction.Direction is TronTransferDirectionEnum.Outbound)
                                    {
                                        payOrderDeposit.CreditBank = transaction.Id;
                                        payOrderDeposit.CreditAcctNo = transaction.From;
                                        payOrderDeposit.CreditAcctName = "";
                                        payOrderDeposit.CreditAmount = transaction.Amount;
                                        payOrderDeposit.Type = "CRDT";

                                        var newPayOrderDepositId = await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                        newPayOrderDepositIdList.Add(newPayOrderDepositId);
                                    }
                                    // Debit
                                    else if (transaction.Direction is TronTransferDirectionEnum.Inbound)
                                    {
                                        payOrderDeposit.DebitBank = transaction.Id;
                                        payOrderDeposit.DebitAcctNo = transaction.To;
                                        payOrderDeposit.DebitAcctName = "";
                                        payOrderDeposit.DebitAmount = transaction.Amount;
                                        payOrderDeposit.Type = "DBIT";

                                        var newPayOrderDepositId = await _payOrderDepositsMongoService.AddAsync(payOrderDeposit);
                                    }
                                    else
                                    {
                                        NlogLogger.Warn($"Skipped transaction due to unknown direction: {transaction.Direction}, payload: {JsonConvert.SerializeObject(transaction)}");
                                        continue;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            NlogLogger.Error($"Tron USDT 运行错误: {ex.Message}", ex);
                        }
                    }

                    merchantNewPayOrderListDict[merchant.MerchantCode] = newPayOrderDepositIdList;
                }
                catch (Exception ex)
                {
                    NlogLogger.Error($"Error processing merchant {merchant.MerchantCode}: {ex.Message}", ex);
                }
            });

            foreach (var (key, value) in merchantNewPayOrderListDict)
            {
                if (value is { Count: > 0 })
                {
                    await _kafkaProducer.ProduceAsync(KafkaTopics.PayOrderCrypto, key, new PayOrderCryptoPublishDto
                    {
                        MerchantCode = key,
                        PaymentType = PayMentTypeEnum.USDT_TRC20,
                        PayOrderDepositIds = value,
                    });
                }
            }

            return Ok(merchantNewPayOrderListDict);
        }
    }
}
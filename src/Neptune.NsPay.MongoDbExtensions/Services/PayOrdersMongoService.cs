using Abp.Collections.Extensions;
using Abp.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.Common;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantDashboard.Dtos;
using Neptune.NsPay.Merchants;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PayOrders.Dtos;
using System.Data;
using System.Diagnostics;
using System.Linq.Dynamic.Core;

namespace Neptune.NsPay.MongoDbExtensions.Services
{
    public class PayOrdersMongoService : MongoBaseService<PayOrdersMongoEntity>, IPayOrdersMongoService, IDisposable
    {
        public PayOrdersMongoService()
        { }

        public async Task UpdatePayOrderMentByOrderId(string id, int payMenyId, PayMentTypeEnum payType)
        {
            var result = await DB.Update<PayOrdersMongoEntity>()
                                 .MatchID(id)
                                 .Modify(r => r.PayMentId, payMenyId)
                                 .Modify(r => r.PayType, payType)
                                 .ExecuteAsync();
        }

        public async Task UpdateSuccesByOrderId(string id, decimal tradeMoney)
        {
            var transferTime = DateTime.Now;
            var transferUnixTime = TimeHelper.GetUnixTimeStamp(transferTime);
            var result = await DB.Update<PayOrdersMongoEntity>()
                                 .MatchID(id)
                                 .Modify(r => r.TradeMoney, tradeMoney)
                                 .Modify(r => r.OrderStatus, PayOrderOrderStatusEnum.Completed)
                                 .Modify(r => r.TransactionTime, transferTime)
                                 .Modify(r => r.TransactionUnixTime, transferUnixTime)
                                 .ExecuteAsync();
        }

        public async Task UpdateScInfoByOrderId(string id, string scCode, string scSeri)
        {
            var transferTime = DateTime.Now;
            var transferUnixTime = TimeHelper.GetUnixTimeStamp(transferTime);
            var result = await DB.Update<PayOrdersMongoEntity>()
                                 .MatchID(id)
                                 .Modify(r => r.ScCode, scCode)
                                 .Modify(r => r.ScSeri, scSeri)
                                 .Modify(r => r.OrderStatus, PayOrderOrderStatusEnum.NotPaid)
                                 .Modify(r => r.TransactionTime, transferTime)
                                 .Modify(r => r.TransactionUnixTime, transferUnixTime)
                                 .ExecuteAsync();
        }

        public async Task UpdateOrderStatusByOrderId(string id, PayOrderOrderStatusEnum status, decimal tradeMoney, string errorMsg)
        {
            var transferTime = DateTime.Now;
            var transferUnixTime = TimeHelper.GetUnixTimeStamp(transferTime);
            var result = await DB.Update<PayOrdersMongoEntity>()
                                 .MatchID(id)
                                 .Modify(r => r.TradeMoney, tradeMoney)
                                 .Modify(r => r.ErrorMsg, errorMsg)
                                 .Modify(r => r.OrderStatus, status)
                                 .Modify(r => r.TransactionTime, transferTime)
                                 .Modify(r => r.TransactionUnixTime, transferUnixTime)
                                 .ExecuteAsync();
        }

        public async Task UpdateScoreStatus(PayOrdersMongoEntity payOrdersEntity)
        {
            var transferTime = DateTime.Now;
            var transferUnixTime = TimeHelper.GetUnixTimeStamp(transferTime);
            var result = await DB.Update<PayOrdersMongoEntity>()
                                 .MatchID(payOrdersEntity.ID)
                                 .Modify(r => r.ScoreNumber, payOrdersEntity.ScoreNumber)
                                 .Modify(r => r.ScoreStatus, payOrdersEntity.ScoreStatus)
                                 .Modify(r => r.TransactionTime, transferTime)
                                 .Modify(r => r.TransactionUnixTime, transferUnixTime)
                                 .ExecuteAsync();
        }

        public async Task<PayOrdersMongoEntity?> GetPayOrderByRemark(string merchantCode, PayMentTypeEnum paytype, string remark, decimal money)
        {
            var dateNow = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddMinutes(-300));
            var result = new List<PayOrdersMongoEntity>();
            if (!merchantCode.IsNullOrEmpty())
            {
                if (merchantCode == NsPayRedisKeyConst.NsPay)
                {
                    result = await DB.Find<PayOrdersMongoEntity>()
                                    .Match(f => f.Gte(a => a.CreationUnixTime, dateNow)
                                                  & f.Eq(a => a.MerchantType, MerchantTypeEnum.External)
                                                  & f.Eq(a => a.PayType, paytype)
                                                  & f.In(a => a.OrderStatus, new[] { PayOrderOrderStatusEnum.NotPaid, PayOrderOrderStatusEnum.TimeOut })
                                                  & f.Eq(a => a.OrderMoney, money)
                                              )
                                    .ExecuteAsync();
                }
                else
                {
                    result = await DB.Find<PayOrdersMongoEntity>()
                                   .Match(f => f.Gte(a => a.CreationUnixTime, dateNow)
                                                 & f.Eq(a => a.MerchantCode, merchantCode)
                                                 & f.Eq(a => a.PayType, paytype)
                                                 & f.In(a => a.OrderStatus, new[] { PayOrderOrderStatusEnum.NotPaid, PayOrderOrderStatusEnum.TimeOut })
                                                 & f.Eq(a => a.OrderMoney, money)
                                             )
                                   .ExecuteAsync();
                }
            }
            else
            {
                result = await DB.Find<PayOrdersMongoEntity>()
                    .Match(f => f.Gte(a => a.CreationUnixTime, dateNow)
                                  & f.Eq(a => a.PayType, paytype)
                                  & f.In(a => a.OrderStatus, new[] { PayOrderOrderStatusEnum.NotPaid, PayOrderOrderStatusEnum.TimeOut })
                                  & f.Eq(a => a.OrderMoney, money)
                              )
                    .ExecuteAsync();
            }
            var info = !string.IsNullOrEmpty(remark) ?
                result.FirstOrDefault(r => remark.ToLower().Trim().Replace(" ", "").Contains(r.OrderMark.ToLower().Trim().Replace(" ", ""))) :
                result.FirstOrDefault();
            return info;
        }

        public async Task<PayOrdersMongoEntity> GetPayOrderByOrderId(string orderId)
        {
            var result = await DB.Find<PayOrdersMongoEntity>().OneAsync(orderId);
            return result;
        }

        public async Task<List<PayOrdersMongoEntity>> GetPayOrderByOrderIdList(List<string> orderIdList)
        {
            var result = await DB.Find<PayOrdersMongoEntity>().Match(u => u.In(r => r.ID, orderIdList)).ExecuteAsync();
            return result;
        }

        public async Task<PayOrdersMongoEntity> GetPayOrderByOrderNumber(string merchantCode, string orderNumber)
        {
            var result = await DB.Find<PayOrdersMongoEntity>()
                                 .Match(r => r.MerchantCode == merchantCode && r.OrderNumber == orderNumber)
                                 .ExecuteSingleAsync();
            return result;
        }

        public async Task<PayOrdersMongoEntity> GetPayOrderByOrderNumber(List<string> merchantCode, string orderNumber)
        {
            var result = new PayOrdersMongoEntity();
            var creationTime = DateTime.Now.AddDays(-10);
            var test = TimeHelper.GetUnixTimeStamp(creationTime);
            if (merchantCode.Count == 1)
            {
                result = await DB.Find<PayOrdersMongoEntity>()
                        .Match(r =>
                                   r.Eq(e => e.MerchantCode, merchantCode.FirstOrDefault())
                                   & r.Eq(e => e.OrderNumber, orderNumber)
                                   & r.Ne(e => e.OrderStatus, PayOrderOrderStatusEnum.Completed)
                                   & r.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(creationTime))
                        )
                        .ExecuteSingleAsync();
            }
            else
            {
                result = await DB.Find<PayOrdersMongoEntity>()
                          .Match(r =>
                                     r.In(e => e.MerchantCode, merchantCode)
                                     & r.Eq(e => e.OrderNumber, orderNumber)
                                     & r.Ne(e => e.OrderStatus, PayOrderOrderStatusEnum.Completed)
                                     & r.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(creationTime))
                          )
                          .ExecuteSingleAsync();
            }
            return result;
        }

        public async Task<PayOrdersMongoEntity?> GetPayOrderByOrderNumber(int merchantCodeId, string orderNumber)
        {
            var result = await DB.Find<PayOrdersMongoEntity>()
                                 .Match(r => r.MerchantId == merchantCodeId && r.OrderNumber == orderNumber)
                                 .ExecuteSingleAsync();
            return result;
        }

        public async Task<List<PayOrdersMongoEntity>> GetPayOrderByPayMentId(int PayMentId, PayOrderOrderStatusEnum orderStatus, DateTime creationTimeFrom)
        {
            var unixTime = TimeHelper.GetUnixTimeStamp(creationTimeFrom);
            var result = await DB.Find<PayOrdersMongoEntity>()
                                 .Match(r => r.CreationUnixTime > unixTime && r.OrderStatus == orderStatus && r.PayMentId == PayMentId)
                                 .ExecuteAsync();
            return result;
        }

        public async Task<List<PayOrdersMongoEntity>> GetPayOrderByCompletedList(string merchantCode, DateTime dateTime, MerchantTypeEnum merchantTypeEnum)
        {
            var unixTime = TimeHelper.GetUnixTimeStamp(dateTime);
            var endTime = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddSeconds(-30));
            if (merchantTypeEnum == MerchantTypeEnum.Internal)
            {
                var result = await DB.Find<PayOrdersMongoEntity>()
                                   .ManyAsync(r => r.MerchantCode == merchantCode && r.OrderStatus == PayOrderOrderStatusEnum.Completed && r.ScoreNumber <= 8 && r.ScoreStatus != PayOrderScoreStatusEnum.Completed && r.CreationUnixTime >= unixTime && r.TransactionUnixTime <= endTime);
                return result;
            }
            else
            {
                var result = await DB.Find<PayOrdersMongoEntity>()
                   .ManyAsync(r => r.MerchantType == MerchantTypeEnum.External && r.OrderStatus == PayOrderOrderStatusEnum.Completed && r.ScoreNumber <= 8 && r.ScoreStatus != PayOrderScoreStatusEnum.Completed && r.CreationUnixTime >= unixTime && r.TransactionUnixTime <= endTime);
                return result;
            }
        }

        public async Task<List<PayOrdersMongoEntity>> GetPayOrderByCompletedList(string merchantCode, DateTime dateTime)
        {
            var unixTime = TimeHelper.GetUnixTimeStamp(dateTime);
            var endTime = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddSeconds(-5));

            var result = await DB.Find<PayOrdersMongoEntity>()
                               .ManyAsync(r => r.MerchantCode == merchantCode && r.OrderStatus == PayOrderOrderStatusEnum.Completed && r.ScoreNumber <= 8 && r.ScoreStatus != PayOrderScoreStatusEnum.Completed && r.CreationUnixTime >= unixTime && r.TransactionUnixTime <= endTime);
            return result;
        }

        public async Task<long> UpdatePayOrderByFailedList(string merchantCode, DateTime dateTime, MerchantTypeEnum merchantTypeEnum)
        {
            var unixTime = TimeHelper.GetUnixTimeStamp(dateTime);
            var startTime = TimeHelper.GetUnixTimeStamp(dateTime.AddMinutes(-120));
            var transferTime = DateTime.Now;
            var transferUnixTime = TimeHelper.GetUnixTimeStamp(transferTime);
            if (merchantTypeEnum == MerchantTypeEnum.Internal)
            {
                var bulkUpdate = DB.Update<PayOrdersMongoEntity>()
                                   .Match(r => r.CreationUnixTime >= startTime && r.CreationUnixTime <= unixTime && r.MerchantCode == merchantCode && r.OrderStatus < PayOrderOrderStatusEnum.Failed && r.PayType != PayMentTypeEnum.ScratchCards)
                                   .Modify(r => r.TransactionTime, transferTime)
                                   .Modify(r => r.TransactionUnixTime, transferUnixTime)
                                   .Modify(r => r.OrderStatus, PayOrderOrderStatusEnum.TimeOut)
                                   .AddToQueue();
                var result = await bulkUpdate.ExecuteAsync();
                return result.ModifiedCount;
            }
            else
            {
                var bulkUpdate = DB.Update<PayOrdersMongoEntity>()
                                   .Match(r => r.CreationUnixTime >= startTime && r.CreationUnixTime <= unixTime && r.MerchantType == MerchantTypeEnum.External && r.OrderStatus < PayOrderOrderStatusEnum.Failed && r.PayType != PayMentTypeEnum.ScratchCards)
                                   .Modify(r => r.TransactionTime, transferTime)
                                   .Modify(r => r.TransactionUnixTime, transferUnixTime)
                                   .Modify(r => r.OrderStatus, PayOrderOrderStatusEnum.TimeOut)
                                   .AddToQueue();
                var result = await bulkUpdate.ExecuteAsync();
                return result.ModifiedCount;
            }
        }

        public async Task<List<PayOrdersMongoEntity>> GetPayOrderByDateRange(DateTime startDate, DateTime endDate, string merchantCode = "")
        {
            if (!string.IsNullOrEmpty(merchantCode))
            {
                var result = await DB.Find<PayOrdersMongoEntity>()
                                    .ManyAsync(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                                  & f.Lte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate))
                                                  & f.Eq(a => a.MerchantCode, merchantCode));
                return result.ToList();
            }
            else
            {
                var result = await DB.Find<PayOrdersMongoEntity>()
                    .ManyAsync(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                  & f.Lte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate)));
                return result.ToList();
            }
        }

        public async Task<List<PayOrdersMongoEntity>> GetPayOrderProjectionsByDateRange(DateTime startDate, DateTime endDate, string merchantCode = "")
        {
            if (!string.IsNullOrEmpty(merchantCode))
            {
                var result = await DB.Find<PayOrdersMongoEntity>()
                                    .Match(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                                & f.Lte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate))
                                                & f.Eq(a => a.MerchantCode, merchantCode))
                                    .Project(a => a.Include("OrderStatus").Include("FeeMoney").Include("OrderMoney").Include("PaymentChannel"))
                                    .ExecuteCursorAsync();
                return result.ToList();
            }
            else
            {
                var result = await DB.Find<PayOrdersMongoEntity>()
                                    .Match(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                                & f.Lte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate)))
                                    .Project(a => a.Include("OrderStatus").Include("FeeMoney").Include("OrderMoney").Include("PaymentChannel"))
                                    .ExecuteCursorAsync();
                return result.ToList();
            }
        }

        public async Task<List<PayOrdersMongoEntity>> GetPayOrderProjectionsByCardNumberDateRange(DateTime startDate, DateTime endDate, List<int> cardNumbers)
        {
            if (cardNumbers != null && cardNumbers.Count > 0)
            {
                if (cardNumbers.Count == 1)
                {
                    var paymentId = cardNumbers.FirstOrDefault();
                    var result = await DB.Find<PayOrdersMongoEntity>()
                                        .Match(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                                    & f.Lte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate))
                                                    & f.Eq(a => a.OrderStatus, PayOrderOrderStatusEnum.Completed)
                                                    & f.In(a => a.PaymentChannel, new List<PaymentChannelEnum> { PaymentChannelEnum.ScanBank, PaymentChannelEnum.OnlineBank, PaymentChannelEnum.OnlineBank })
                                                    & f.Eq(a => a.PayMentId, paymentId))
                                        .Project(a => a.Include("OrderStatus").Include("FeeMoney").Include("OrderMoney").Include("PaymentChannel").Include("PayType").Include("PayMentId"))
                                        .ExecuteCursorAsync();
                    return result.ToList();
                }
                else
                {
                    var result = await DB.Find<PayOrdersMongoEntity>()
                                        .Match(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                                    & f.Lte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate))
                                                    & f.Eq(a => a.OrderStatus, PayOrderOrderStatusEnum.Completed)
                                                    & f.In(a => a.PaymentChannel, new List<PaymentChannelEnum> { PaymentChannelEnum.ScanBank, PaymentChannelEnum.OnlineBank, PaymentChannelEnum.OnlineBank })
                                                    & f.In(a => a.PayMentId, cardNumbers))
                                        .Project(a => a.Include("OrderStatus").Include("FeeMoney").Include("OrderMoney").Include("PaymentChannel").Include("PayType").Include("PayMentId"))
                                        .ExecuteCursorAsync();
                    return result.ToList();
                }
            }
            else
            {
                var result = await DB.Find<PayOrdersMongoEntity>()
                                    .Match(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                                & f.Lte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate))
                                                & f.Eq(a => a.OrderStatus, PayOrderOrderStatusEnum.Completed)
                                                & f.In(a => a.PaymentChannel, new List<PaymentChannelEnum> { PaymentChannelEnum.ScanBank, PaymentChannelEnum.OnlineBank, PaymentChannelEnum.OnlineBank }))
                                    .Project(a => a.Include("OrderStatus").Include("FeeMoney").Include("OrderMoney").Include("PaymentChannel").Include("PayType").Include("PayMentId"))
                                    .ExecuteCursorAsync();
                return result.ToList();
            }
        }

        public async Task<List<PayOrdersMongoEntity>> GetPayOrderByDateRangeAndStatus(DateTime startDate, DateTime endDate, PayOrderOrderStatusEnum Status)
        {
            var result = await DB.Find<PayOrdersMongoEntity>()
                                .ManyAsync(f => f.Gte(a => a.TransactionUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                              & f.Lte(a => a.TransactionUnixTime, TimeHelper.GetUnixTimeStamp(endDate)) & f.Eq(a => a.OrderStatus, Status));
            return result.ToList();
        }

        public async Task<PayOrderPageResultDto<PayOrdersMongoEntity>> GetAllWithPagination(GetAllPayOrdersInput input, List<int> merchantIds, List<int> paymentIds = null)
        {
            var orderType = input.OrderTypeFilter.HasValue ? (PayOrderOrderTypeEnum)input.OrderTypeFilter : default;
            var paymentChannel = input.OrderPayTypeFilter.HasValue ? (PaymentChannelEnum)input.OrderPayTypeFilter : default;
            var orderStatus = input.OrderStatusFilter.HasValue ? (PayOrderOrderStatusEnum)input.OrderStatusFilter : default;
            var scoreStatus = input.ScoreStatusFilter.HasValue ? (PayOrderScoreStatusEnum)input.ScoreStatusFilter : default;

            if (input.OrderCreationTimeStartDate.HasValue)
            {
                input.OrderCreationTimeStartDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeStartDate.Value, input.UtcTimeFilter);
            }
            if (input.OrderCreationTimeEndDate.HasValue)
            {
                input.OrderCreationTimeEndDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeEndDate.Value, input.UtcTimeFilter);
            }

            var builder = Builders<PayOrdersMongoEntity>.Filter;

            FilterDefinition<PayOrdersMongoEntity> userMerchantCodeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> paymentIdFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> orderFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> merchantCodeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> orderNoFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> orderMarkFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> orderPayTypeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> orderTypeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> orderStatusFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> scoreStatusFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> minOrderMoneyFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> maxOrderMoneyFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> minCreationTimeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> maxCreationTimeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> orderIdFilter = builder.Empty;

            if (merchantIds.Count == 1)
            {
                userMerchantCodeFilter = builder.Eq(e => e.MerchantId, merchantIds.FirstOrDefault());
            }
            else
            {
                userMerchantCodeFilter = builder.In(e => e.MerchantId, merchantIds);
            }

            if (paymentIds is not null)
                paymentIdFilter = builder.In(e => e.PayMentId, paymentIds);

            if (input.OrderCreationTimeStartDate != null)
                minCreationTimeFilter = builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.OrderCreationTimeStartDate));

            if (input.OrderCreationTimeEndDate != null)
                maxCreationTimeFilter = builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.OrderCreationTimeEndDate));

            if (!input.Filter.IsNullOrEmpty())
            {
                var textfilter = "\"" + input.Filter + "\"";
                orderFilter = builder.Text(textfilter);
            }

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
                merchantCodeFilter = builder.Eq(e => e.MerchantCode, input.MerchantCodeFilter);

            if (!input.OrderNoFilter.IsNullOrEmpty())
                orderNoFilter = builder.Eq(e => e.OrderNumber, input.OrderNoFilter);

            if (!input.OrderMarkFilter.IsNullOrEmpty())
                orderMarkFilter = builder.Regex(e => e.OrderMark, input.OrderMarkFilter);

            if (input.OrderPayTypeFilter.HasValue && input.OrderPayTypeFilter > -1)
                orderPayTypeFilter = builder.Eq(e => e.PaymentChannel, paymentChannel);

            if (input.OrderTypeFilter.HasValue && input.OrderTypeFilter > -1)
                orderTypeFilter = builder.Eq(e => e.OrderType, orderType);

            if (input.OrderStatusFilter.HasValue && input.OrderStatusFilter > -1)
                orderStatusFilter = builder.Eq(e => e.OrderStatus, orderStatus);

            if (input.ScoreStatusFilter.HasValue && input.ScoreStatusFilter > -1)
                scoreStatusFilter = builder.Eq(e => e.ScoreStatus, scoreStatus);

            if (input.MinOrderMoneyFilter != null)
                minOrderMoneyFilter = builder.Gte(e => e.OrderMoney, input.MinOrderMoneyFilter);

            if (input.MaxOrderMoneyFilter != null)
                maxOrderMoneyFilter = builder.Lte(e => e.OrderMoney, input.MaxOrderMoneyFilter);

            if (!input.OrderId.IsNullOrEmpty())
                orderIdFilter = builder.Eq(e => e.ID, input.OrderId);

            var filter = builder.And(
                maxCreationTimeFilter,
                minCreationTimeFilter,
                userMerchantCodeFilter,
                paymentIdFilter,
                orderFilter,
                merchantCodeFilter,
                orderNoFilter,
                orderMarkFilter,
                orderPayTypeFilter,
                orderTypeFilter,
                orderStatusFilter,
                scoreStatusFilter,
                minOrderMoneyFilter,
                maxOrderMoneyFilter,
                orderIdFilter
            );

            decimal SumOrderMoney = 0;
            decimal SumFeeMoney = 0;
            int totalCount = 0;
            List<Task> pullTaskList = new List<Task>();
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            pullTaskList.Add(Task.Run(async () =>
            {
                try
                {
                    var pipeline = new[] {
                        new BsonDocument("$match", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<PayOrdersMongoEntity>(), BsonSerializer.SerializerRegistry)),
                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", BsonNull.Value },
                            { "totalCount", new BsonDocument("$sum", 1) },
                            { "sumOrderMoney", new BsonDocument("$sum", "$OrderMoney") },
                            { "sumFeeMoney", new BsonDocument("$sum", "$FeeMoney") }
                        })
                    };

                    var sumResult = await DB.Collection<PayOrdersMongoEntity>().Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

                    if (sumResult != null)
                    {
                        SumOrderMoney = sumResult["sumOrderMoney"].ToDecimal();
                        SumFeeMoney = sumResult["sumFeeMoney"].ToDecimal();
                        totalCount = sumResult["totalCount"].ToInt32();
                    }
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("GetAllWithPagination - Error", ex);
                }
            }));

            stopWatch.Stop();

            Console.WriteLine("Get Group By Time Taken " + stopWatch.ElapsedMilliseconds + "ms");

            int skip = input.SkipCount;
            int limit = input.MaxResultCount;
            int pageNumber = (skip / limit) + 1;
            List<PayOrdersMongoEntity> reponseResult = new List<PayOrdersMongoEntity>();

            stopWatch.Restart();
            pullTaskList.Add(Task.Run(async () =>
            {
                try
                {
                    var recordSorting = Builders<PayOrdersMongoEntity>.Sort.Descending(x => x.CreationUnixTime);
                    var skip = (pageNumber - 1) * limit;
                    var response = await DB.Collection<PayOrdersMongoEntity>()
                                     .Find(filter)
                                     .Sort(recordSorting)
                                     .Skip(skip)
                                     .Limit(limit)
                                     .ToListAsync();

                    reponseResult.AddRange(response);
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("GetAllWithPagination- Error", ex);
                }
            }));

            stopWatch.Stop();

            Console.WriteLine("Get Paging Time Taken " + stopWatch.ElapsedMilliseconds + "ms");

            await Task.WhenAll(pullTaskList);

            return new PayOrderPageResultDto<PayOrdersMongoEntity>
            {
                Items = reponseResult,
                TotalCount = totalCount,
                FeeMoneyTotal = SumFeeMoney,
                OrderMoneyTotal = SumOrderMoney,
            };
        }

        public async Task<List<PayOrdersMongoEntity>> GetAll(GetAllPayOrdersForExcelInput input, List<int> merchantIds)
        {
            var filter = BuildExcelPayOrderFilter(input, merchantIds);

            var response = await DB.Find<PayOrdersMongoEntity>()
                          .Match(filter)
                          .Sort(s => s.CreationUnixTime, Order.Descending)
                          .ExecuteAsync();

            return response;
        }

        public async Task<List<PayOrdersMongoEntity>> GetBatchNotifcationCheckBox(List<string> ListOfPayOrderID)
        {
            // Convert string IDs to ObjectId
            var objectIdList = ListOfPayOrderID.Where(id => ObjectId.TryParse(id, out _))
                .Select(id => new ObjectId(id)).ToList();

            if (!objectIdList.Any())
            {
                return new List<PayOrdersMongoEntity>();
            }

            var builder = Builders<PayOrdersMongoEntity>.Filter;
            var filter = builder.In("_id", objectIdList);

            var response = await DB.Find<PayOrdersMongoEntity>()
                .Match(filter)
                .Sort(s => s.CreationUnixTime, Order.Descending)
                .ExecuteAsync();

            return response;
        }

        public async Task<bool> UpdateManyAsync(List<PayOrdersMongoEntity> payOrders)
        {
            if (payOrders == null || !payOrders.Any())
                return false;

            var bulkOperations = DB.Update<PayOrdersMongoEntity>(); // Correct entity type

            foreach (var payOrder in payOrders)
            {
                bulkOperations = bulkOperations
                    .Match(payOrder.ID)  // Match each ID
                    .ModifyWith(payOrder); // Apply updates
            }

            var result = await bulkOperations.ExecuteAsync();
            return result.IsModifiedCountAvailable;
        }

        public async Task<int> GetTotalExcelRecordCount(GetAllPayOrdersForExcelInput input, List<int> merchantIds)
        {
            var filter = BuildExcelPayOrderFilter(input, merchantIds);

            var totalRecords = await DB.Collection<PayOrdersMongoEntity>()
                .CountDocumentsAsync(filter);

            return (int)totalRecords;
        }

        private static FilterDefinition<PayOrdersMongoEntity> BuildExcelPayOrderFilter(GetAllPayOrdersForExcelInput input, List<int> merchantIds)
        {
            var builder = Builders<PayOrdersMongoEntity>.Filter;
            var filters = new List<FilterDefinition<PayOrdersMongoEntity>>();

            DateTime? startDate = input.OrderCreationTimeStartDate.HasValue
                   ? CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeStartDate.Value, input.UtcTimeFilter)
                   : (DateTime?)null;

            DateTime? endDate = input.OrderCreationTimeEndDate.HasValue
                ? CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeEndDate.Value, input.UtcTimeFilter)
                : (DateTime?)null;

            if (startDate != null)
            {
                filters.Add(builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)startDate)));
            }

            if (endDate != null)
            {
                filters.Add(builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)endDate)));
            }

            if (merchantIds.Count == 1)
            {
                filters.Add(builder.Eq(e => e.MerchantId, merchantIds.First()));
            }
            else
            {
                filters.Add(builder.In(e => e.MerchantId, merchantIds));
            }

            if (!input.Filter.IsNullOrEmpty())
            {
                filters.Add(builder.Text(input.Filter));
            }

            if (!input.OrderNoFilter.IsNullOrEmpty())
            {
                filters.Add(builder.Eq(e => e.OrderNo, input.OrderNoFilter));
            }

            if (!input.OrderMarkFilter.IsNullOrEmpty())
            {
                filters.Add(builder.Regex(e => e.OrderMark, input.OrderMarkFilter));
            }

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
            {
                filters.Add(builder.Eq(e => e.MerchantCode, input.MerchantCodeFilter));
            }

            if (input.OrderPayTypeFilter.HasValue && input.OrderPayTypeFilter > -1)
            {
                filters.Add(builder.Eq(e => e.PaymentChannel, (PaymentChannelEnum)input.OrderPayTypeFilter));
            }

            if (input.OrderTypeFilter.HasValue && input.OrderTypeFilter > -1)
            {
                filters.Add(builder.Eq(e => e.OrderType, (PayOrderOrderTypeEnum)input.OrderTypeFilter));
            }

            if (input.OrderStatusFilter.HasValue && input.OrderStatusFilter > -1)
            {
                filters.Add(builder.Eq(e => e.OrderStatus, (PayOrderOrderStatusEnum)input.OrderStatusFilter));
            }

            if (input.ScoreStatusFilter.HasValue && input.ScoreStatusFilter > -1)
            {
                filters.Add(builder.Eq(e => e.ScoreStatus, (PayOrderScoreStatusEnum)input.ScoreStatusFilter));
            }

            if (input.MinOrderMoneyFilter != null)
            {
                filters.Add(builder.Gte(e => e.OrderMoney, input.MinOrderMoneyFilter));
            }

            if (input.MaxOrderMoneyFilter != null)
            {
                filters.Add(builder.Lte(e => e.OrderMoney, input.MaxOrderMoneyFilter));
            }

            return filters.Any() ? builder.And(filters) : builder.Empty;
        }

        public async Task<List<string>> GetPayOrdersByFilter(string order, string userName, List<string> merchants)
        {
            var result = new List<PayOrdersMongoEntity>();
            var builder = Builders<PayOrdersMongoEntity>.Filter;

            FilterDefinition<PayOrdersMongoEntity> userNameFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> orderFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> userMerchantFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> orderStatusFilter = builder.Empty;

            if (!order.IsNullOrEmpty())
                orderFilter = builder.Eq(e => e.OrderNumber, order);

            if (!userName.IsNullOrEmpty())
                userNameFilter = builder.Eq(e => e.UserNo, userName);

            if (merchants.Count == 1)
                userMerchantFilter = builder.Eq(e => e.MerchantCode, merchants.FirstOrDefault());
            else
                userMerchantFilter = builder.In(e => e.MerchantCode, merchants);

            orderStatusFilter = builder.Eq(e => e.OrderStatus, PayOrderOrderStatusEnum.Completed);

            var filter = builder.And(userNameFilter, orderFilter, userMerchantFilter, orderStatusFilter);

            result = await DB.Find<PayOrdersMongoEntity>()
                     .Match(filter)
                     .ExecuteAsync();

            if (result.Count > 0)
            {
                return result.Select(s => s.ID).ToList();
            }
            return new List<string>();
        }

        public async Task<IEnumerable<PayOrdersMongoEntity>> GetPayOrderListForExcelAsync(List<string> merchants, GetAllPayOrderDepositsForExcelInput input)
        {
            if (input.MinTransactionTimeFilter.HasValue)
            {
                input.MinTransactionTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MinTransactionTimeFilter.Value, input.UtcTimeFilter);
            }
            if (input.MaxTransactionTimeFilter.HasValue)
            {
                input.MaxTransactionTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MaxTransactionTimeFilter.Value, input.UtcTimeFilter);
            }
            var builder = Builders<PayOrdersMongoEntity>.Filter;

            FilterDefinition<PayOrdersMongoEntity> userMerchantCodeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> minCreationTimeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> maxCreationTimeFilter = builder.Empty;

            if (merchants.Count == 1)
                userMerchantCodeFilter = builder.Eq(e => e.MerchantCode, merchants.FirstOrDefault());
            else
                userMerchantCodeFilter = builder.In(e => e.MerchantCode, merchants);

            if (input.MinTransactionTimeFilter != null)
                minCreationTimeFilter = builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MinTransactionTimeFilter));

            if (input.MaxTransactionTimeFilter != null)
                maxCreationTimeFilter = builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MaxTransactionTimeFilter));

            var filter = builder.And(userMerchantCodeFilter, minCreationTimeFilter, maxCreationTimeFilter);
            var response = await DB.Find<PayOrdersMongoEntity>()
                    .Match(filter)
                    .Sort(s => s.CreationTime, Order.Ascending)
                    .ExecuteAsync();
            return response;
        }

        public async Task<PayOrdersMongoEntity> GetPayOrderForExcelAsync(List<string> merchants, GetAllPayOrderDepositsForExcelInput input, string orderId)
        {
            if (input.MinTransactionTimeFilter.HasValue)
            {
                input.MinTransactionTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MinTransactionTimeFilter.Value, input.UtcTimeFilter);
            }
            if (input.MaxTransactionTimeFilter.HasValue)
            {
                input.MaxTransactionTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MaxTransactionTimeFilter.Value, input.UtcTimeFilter);
            }
            var builder = Builders<PayOrdersMongoEntity>.Filter;

            FilterDefinition<PayOrdersMongoEntity> userMerchantCodeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> minCreationTimeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> maxCreationTimeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> IdFilter = builder.Empty;

            if (merchants.Count == 1)
                userMerchantCodeFilter = builder.Eq(e => e.MerchantCode, merchants.FirstOrDefault());
            else
                userMerchantCodeFilter = builder.In(e => e.MerchantCode, merchants);

            if (input.MinTransactionTimeFilter != null)
                minCreationTimeFilter = builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MinTransactionTimeFilter));

            if (input.MaxTransactionTimeFilter != null)
                maxCreationTimeFilter = builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MaxTransactionTimeFilter));

            IdFilter = builder.Eq(e => e.ID, orderId);

            var filter = builder.And(IdFilter, userMerchantCodeFilter, minCreationTimeFilter, maxCreationTimeFilter);
            var response = await DB.Find<PayOrdersMongoEntity>()
                    .Match(filter)
                    .Sort(s => s.CreationUnixTime, Order.Descending)
                    .ExecuteSingleAsync();
            return response;
        }

        public async Task<List<CurrentPayOrderCashInByType>> GetAllforMerchantDashboardSummary(GetAllMerchantDashboardInput input, List<int> merchantId)
        {
            var response = new List<CurrentPayOrderCashInByType>();

            var builder = Builders<PayOrdersMongoEntity>.Filter;

            FilterDefinition<PayOrdersMongoEntity> userMerchantIdFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> minCreationTimeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> maxCreationTimeFilter = builder.Empty;
            FilterDefinition<PayOrdersMongoEntity> orderStatusFilter = builder.Empty;

            if (input.OrderCreationTimeStartDate.HasValue)
            {
                input.OrderCreationTimeStartDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeStartDate.Value, "GMT7+");
            }
            if (input.OrderCreationTimeEndDate.HasValue)
            {
                input.OrderCreationTimeEndDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeEndDate.Value, "GMT7+");
            }

            userMerchantIdFilter = builder.In(e => e.MerchantId, merchantId);
            minCreationTimeFilter = builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(input.OrderCreationTimeStartDate.Value));
            maxCreationTimeFilter = builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(input.OrderCreationTimeEndDate.Value));
            orderStatusFilter = builder.Eq(e => e.OrderStatus, PayOrderOrderStatusEnum.Completed);

            var filter = builder.And(
                                userMerchantIdFilter,
                                minCreationTimeFilter,
                                maxCreationTimeFilter,
                                orderStatusFilter
                                );

            var collection = DB.Collection<PayOrdersMongoEntity>();
            var pipeline = new[] {
                new BsonDocument("$match", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<PayOrdersMongoEntity>(), BsonSerializer.SerializerRegistry)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument
                        {
                            { "merchantId", "$MerchantId" },
                            { "channel" , "$PaymentChannel"}
                        }
                    },
                    { "totalCount", new BsonDocument("$sum", 1) },
                    { "sumOrderMoney", new BsonDocument("$sum", "$OrderMoney") },
                    { "sumFeeMoney", new BsonDocument("$sum", "$FeeMoney") }
                })
            };

            var sumResult = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            if (sumResult.Count > 0)
            {
                var result = sumResult.Select(s => new CurrentPayOrderCashInByType
                {
                    MerchantId = s["_id"]["merchantId"].ToInt64(),
                    PaymentChannel = s["_id"]["channel"].ToInt32(),
                    CashInByType = s["sumOrderMoney"].AsDecimal,
                    CashInFeeByType = s["sumFeeMoney"].AsDecimal,
                    CashInCountByType = s["totalCount"].ToInt32()
                }).ToList();

                response = result.GroupBy(g => new
                {
                    merchantId = g.MerchantId,
                    groupType = GetGroupKey((int)g.PaymentChannel)
                }
                                )
                                .Select(s => new CurrentPayOrderCashInByType
                                {
                                    MerchantId = s.Key.merchantId,
                                    PaymentChannel = s.Key.groupType,
                                    CashInByType = s.Sum(a => a.CashInByType),
                                    CashInFeeByType = s.Sum(a => a.CashInFeeByType),
                                    CashInCountByType = s.Sum(a => a.CashInCountByType)
                                }).ToList();
            }

            return response;
        }

        private static int GetGroupKey(int type)
        {
            var groupThree = new List<int>() { 3, 4, 5 };
            if (groupThree.Contains(type))
            {
                return 3;
            }
            if (type == 6)
                return 4;
            if (type == 7)
                return 5;
            return type;
        }

        public async Task<PayOrdersMongoEntity?> GetPayOrderByOrderNumberAmt(decimal amount, string orderNumber)
        {
            var result = await DB.Find<PayOrdersMongoEntity>()
                                 .Match(r => r.OrderMoney == amount
                                    && r.OrderNumber == orderNumber
                                    && r.OrderStatus == PayOrderOrderStatusEnum.Completed
                                 ).ExecuteSingleAsync();
            return result;
        }

        public void Dispose()
        {
        }
    }
}
using Abp.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.Common;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MerchantDashboard.Dtos;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.WithdrawalOrders;
using Neptune.NsPay.WithdrawalOrders.Dtos;

namespace Neptune.NsPay.MongoDbExtensions.Services
{
    public class WithdrawalOrdersMongoService : MongoBaseService<WithdrawalOrdersMongoEntity>, IWithdrawalOrdersMongoService, IDisposable
    {
        public WithdrawalOrdersMongoService()
        {
        }

        public async Task<bool> UpdateNotifyStatus(string Id, int number)
        {
            var result = await DB.Update<WithdrawalOrdersMongoEntity>()
                            .MatchID(Id)
                            .Modify(a => a.NotifyStatus, WithdrawalNotifyStatusEnum.Success)
                            .Modify(a => a.NotifyNumber, number)
                            .ExecuteAsync();
            return result.IsAcknowledged;
        }

        public async Task<bool> UpdateNotifyStatus(string Id, int number , WithdrawalNotifyStatusEnum status)
        {
            var result = await DB.Update<WithdrawalOrdersMongoEntity>()
                            .MatchID(Id)
                            .Modify(a => a.NotifyStatus, status)
                            .Modify(a => a.NotifyNumber, number)
                            .ExecuteAsync();
            return result.IsAcknowledged;
        }

        public async Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByOrderNumber(int merchantCodeId, string orderNumber)
        {
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .Match(r => r.MerchantId == merchantCodeId && r.OrderNumber == orderNumber)
                                 .ExecuteSingleAsync();
            return result;
        }

        public async Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByOrderNumber(string orderNumber)
        {
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .Match(r => r.OrderNumber == orderNumber)
                                 .ExecuteSingleAsync();
            return result;
        }

        public async Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByOrderNumber(string merchantCode, string orderNumber)
        {
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .Match(r => r.MerchantCode == merchantCode && r.OrderNumber == orderNumber)
                                 .ExecuteSingleAsync();
            return result;
        }

        public async Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByOrderId(string merchantCode, string orderId)
        {
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .Match(r => r.MerchantCode == merchantCode && r.ID == orderId)
                                 .ExecuteSingleAsync();
            return result;
        }

        public async Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderByFk(string merchantCode)
        {
            var time = DateTime.Now.AddMinutes(-60);
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .Match(r => r.MerchantCode == merchantCode && r.OrderType == WithdrawalOrderTypeEnum.PlatformFK
                                 && r.OrderStatus == WithdrawalOrderStatusEnum.Success && r.NotifyStatus == WithdrawalNotifyStatusEnum.Wait
                                 && r.CreationUnixTime >= TimeHelper.GetUnixTimeStamp(time))
                                 .ExecuteAsync();
            return result;
        }

        public async Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByDevice(string merchantCode, int deviceId, DateTime startTime)
        {
            var dateTime = TimeHelper.GetUnixTimeStamp(startTime);
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .Match(r => r.MerchantCode == merchantCode && r.OrderStatus == WithdrawalOrderStatusEnum.Pending && r.DeviceId == deviceId && r.CreationUnixTime >= dateTime)
                                 .ExecuteSingleAsync();
            return result;
        }

        public async Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByDevice(int deviceId, DateTime startTime)
        {
            var dateTime = TimeHelper.GetUnixTimeStamp(startTime);
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .Match(r => r.OrderStatus == WithdrawalOrderStatusEnum.Pending && r.DeviceId == deviceId && r.CreationUnixTime >= dateTime)
                                 .ExecuteSingleAsync();
            return result;
        }

        public async Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderCountByDevice(List<int> deviceIds)
        {
            var dateTime = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddHours(-1));
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .Match(f => f.Gte(r => r.CreationUnixTime, dateTime)
                                               & f.In(r => r.OrderStatus, new[] { WithdrawalOrderStatusEnum.Wait, WithdrawalOrderStatusEnum.Pending })
                                               & f.In(r => r.DeviceId, deviceIds))
                                 .Project(a => a.Include("DeviceId"))
                                 .ExecuteCursorAsync();
            return result.ToList();
        }

        public async Task<WithdrawalOrdersMongoEntity?> GetWithdrawOrderByDevice(List<string> merchantCode, int deviceId, DateTime startTime)
        {
            var dateTime = TimeHelper.GetUnixTimeStamp(startTime);
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .Match(r => !merchantCode.Contains(r.MerchantCode) && r.OrderStatus == WithdrawalOrderStatusEnum.Pending && r.DeviceId == deviceId && r.CreationUnixTime >= dateTime)
                                 .ExecuteSingleAsync();
            return result;
        }

        public async Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderProcess(string merchantCode)
        {
            var dateTime = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddMinutes(-300));
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .Match(r => r.MerchantCode == merchantCode && r.OrderType == WithdrawalOrderTypeEnum.NsPayTransfer && (r.OrderStatus == WithdrawalOrderStatusEnum.Wait || r.OrderStatus == WithdrawalOrderStatusEnum.WaitPhone || r.OrderStatus == WithdrawalOrderStatusEnum.PendingProcess || r.OrderStatus == WithdrawalOrderStatusEnum.Pending) && r.CreationUnixTime >= dateTime)
                                 .Project(a => a.Include("OrderMoney"))
                                 .ExecuteCursorAsync();
            return result.ToList();
        }

        public async Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderByWaitPhone(DateTime startTime)
        {
            var dateTime = TimeHelper.GetUnixTimeStamp(startTime);
            var endTimedateTime = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddMinutes(-1));
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .ManyAsync(r => r.OrderType == WithdrawalOrderTypeEnum.NsPayTransfer && r.OrderStatus == WithdrawalOrderStatusEnum.WaitPhone && r.TransactionUnixTime >= dateTime && r.TransactionUnixTime <= endTimedateTime);
            return result;
        }

        public async Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderCallBack(DateTime startTime)
        {
            //var dateTime = TimeHelper.GetUnixTimeStamp(startTime);
            //var result = await DB.Find<WithdrawalOrdersMongoEntity>()
            //                     .ManyAsync(r => r.OrderType == WithdrawalOrderTypeEnum.NsPayTransfer && (r.OrderStatus == WithdrawalOrderStatusEnum.Success || r.OrderStatus == WithdrawalOrderStatusEnum.Fail || r.OrderStatus == WithdrawalOrderStatusEnum.ErrorBank || r.OrderStatus == WithdrawalOrderStatusEnum.ErrorCard) && (r.NotifyStatus == WithdrawalNotifyStatusEnum.Wait || r.NotifyStatus == WithdrawalNotifyStatusEnum.Fail) && r.NotifyNumber < 8 && r.CreationUnixTime >= dateTime);
            //return result;
            var dateTime = TimeHelper.GetUnixTimeStamp(startTime);
            var endTimedateTime = TimeHelper.GetUnixTimeStamp(DateTime.Now.AddMinutes(-1));
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                 .ManyAsync(r => r.OrderType == WithdrawalOrderTypeEnum.NsPayTransfer && r.OrderStatus == WithdrawalOrderStatusEnum.Success && (r.NotifyStatus == WithdrawalNotifyStatusEnum.Wait || r.NotifyStatus == WithdrawalNotifyStatusEnum.Fail) && r.NotifyNumber < 8 && r.TransactionUnixTime >= dateTime && r.TransactionUnixTime <= endTimedateTime);
            return result;
        }

        public async Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderByDateRange(DateTime startDate, DateTime endDate, string merchantCode = "")
        {
            if (!string.IsNullOrEmpty(merchantCode))
            {
                var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                    .ManyAsync(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                  & f.Lt(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate))
                                  & f.Eq(a => a.MerchantCode, merchantCode));
                return result.ToList();
            }
            else
            {
                var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                    .ManyAsync(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                                  & f.Lt(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate)));
                return result.ToList();
            }
        }

        public async Task<List<WithdrawalOrdersMongoEntity>> GetWithdrawOrderByDateRange(DateTime startDate, DateTime endDate, WithdrawalOrderStatusEnum orderStatus)
        {
            var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                                .ManyAsync(f => f.Gte(a => a.TransactionUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                              & f.Lt(a => a.TransactionUnixTime, TimeHelper.GetUnixTimeStamp(endDate)) & f.Eq(a => a.OrderStatus, orderStatus));
            return result.ToList();
        }

        public async Task<List<WithdrawalOrdersMongoEntity>> GetMerchantPendingOrder(string merchantCode = "")
        {
            var pendingStatusList = new List<WithdrawalOrderStatusEnum>() { WithdrawalOrderStatusEnum.Pending, WithdrawalOrderStatusEnum.Wait, WithdrawalOrderStatusEnum.WaitPhone, WithdrawalOrderStatusEnum.PendingProcess };
            if (!string.IsNullOrEmpty(merchantCode))
            {
                var result = await DB.Find<WithdrawalOrdersMongoEntity>()
                    .ManyAsync(f => f.In(a => a.OrderStatus, pendingStatusList)
                                  & f.Eq(a => a.MerchantCode, merchantCode));
                return result.ToList();
            }
            else
            {

                return new List<WithdrawalOrdersMongoEntity>();
            }
        }


        public async Task<WithdrawalOrderPageResultDto<WithdrawalOrdersMongoEntity>> GetAllWithPagination(GetAllWithdrawalOrdersInput input, List<int> merchantIds, List<int> deviceIds)
        {

            var orderStatus = input.OrderStatusFilter.HasValue
                        ? (WithdrawalOrderStatusEnum)input.OrderStatusFilter
                        : default;

            var notifyStatus = input.NotifyStatusFilter.HasValue
                        ? (WithdrawalNotifyStatusEnum)input.NotifyStatusFilter
                        : default;

            if (input.OrderCreationTimeStartDate.HasValue)
            {
                input.OrderCreationTimeStartDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeStartDate.Value, "GMT7+");
            }
            if (input.OrderCreationTimeEndDate.HasValue)
            {
                input.OrderCreationTimeEndDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeEndDate.Value, "GMT7+");
            }

            var builder = Builders<WithdrawalOrdersMongoEntity>.Filter;

            FilterDefinition<WithdrawalOrdersMongoEntity> userMerchantIdFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> minCreationTimeFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> maxCreationTimeFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> orderFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> merchantCodeFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> orderNumberFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> orderStatusFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> notifyStatusFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> benAcctNameFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> benAcctNoFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> deviceFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> releaseStatusFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> minMoneyFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> maxMoneyFilter = builder.Empty;

            userMerchantIdFilter = builder.In(e => e.MerchantId, merchantIds);
            minCreationTimeFilter = builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(input.OrderCreationTimeStartDate.Value));
            maxCreationTimeFilter = builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(input.OrderCreationTimeEndDate.Value));

            if (!input.Filter.IsNullOrEmpty())
            {
                var textfilter = "\"" + input.Filter + "\"";
                orderFilter = builder.Text(textfilter);
            }

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
                merchantCodeFilter = builder.Eq(e => e.MerchantCode, input.MerchantCodeFilter);

            if (!input.OrderNoFilter.IsNullOrEmpty())
                orderNumberFilter = builder.Regex(e => e.OrderNumber, new BsonRegularExpression(input.OrderNoFilter, "i"));

            if (input.OrderStatusFilter.HasValue && input.OrderStatusFilter > -1)
                orderStatusFilter = builder.Eq(e => e.OrderStatus, orderStatus);

            if (input.NotifyStatusFilter.HasValue && input.NotifyStatusFilter > -1)
                notifyStatusFilter = builder.Eq(e => e.NotifyStatus, notifyStatus);

            if (!input.BenAccountNameFilter.IsNullOrEmpty())
                benAcctNameFilter = builder.Eq(e => e.BenAccountName, input.BenAccountNameFilter);

            if (!input.BenAccountNoFilter.IsNullOrEmpty())
                benAcctNoFilter = builder.Eq(e => e.BenAccountNo, input.BenAccountNoFilter);

            if (deviceIds.Count > 0)
                deviceFilter = builder.In(e => e.DeviceId, deviceIds);

            if (input.ReleaseStatus.HasValue)
                releaseStatusFilter = builder.Eq(e => e.ReleaseStatus, input.ReleaseStatus.Value);

            if (input.MinMoneyFilter != null)
                minMoneyFilter = builder.Gte(e => e.OrderMoney, input.MinMoneyFilter);

            if (input.MaxMoneyFilter != null)
                maxMoneyFilter = builder.Lte(e => e.OrderMoney, input.MaxMoneyFilter);

            var filter = builder.And(
                                maxCreationTimeFilter,
                                minCreationTimeFilter,
                                orderNumberFilter,
                                benAcctNameFilter,
                                benAcctNoFilter,
                                orderFilter,
                                userMerchantIdFilter,
                                merchantCodeFilter,
                                orderStatusFilter,
                                notifyStatusFilter,
                                deviceFilter,
                                releaseStatusFilter,
                                minMoneyFilter,
                                maxMoneyFilter
                                );

            int TotalCount = 0;
            decimal SumOrderMoney = 0;
            decimal SumFeeMoney = 0;

            List<Task> taskList = new List<Task>();

            taskList.Add(Task.Run(async () =>
            {
                try
                {
                    var pipeline = new[] {
                new BsonDocument("$match", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<WithdrawalOrdersMongoEntity>(), BsonSerializer.SerializerRegistry)),
                new BsonDocument("$project", new BsonDocument
                    {
                        { nameof(WithdrawalOrdersMongoEntity.ProofContent), 0 } // Exclude 'ProofContent' field
                    }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "totalCount", new BsonDocument("$sum", 1) },
                    { "sumOrderMoney", new BsonDocument("$sum", "$OrderMoney") }, // Sum the "OrderMoney" field
                    { "sumFeeMoney", new BsonDocument("$sum", "$FeeMoney") } // Sum the "FeeMoney" field
                })
            };

                    var sumResult = await DB.Collection<WithdrawalOrdersMongoEntity>().Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

                    if (sumResult != null)
                    {
                        SumOrderMoney = sumResult["sumOrderMoney"].ToDecimal();
                        SumFeeMoney = sumResult["sumFeeMoney"].ToDecimal();
                        TotalCount = sumResult["totalCount"].ToInt32();
                    }
                }
                catch (Exception ex)
                {
                    NlogLogger.Error("GetAllWithPagination", ex);
                }

            }));

            int pageNumber = input.SkipCount == 0 ? 1 : (input.SkipCount / input.MaxResultCount) + 1;
            int limit = input.MaxResultCount;
            List<WithdrawalOrdersMongoEntity> resultResult = new List<WithdrawalOrdersMongoEntity>();

            taskList.Add(Task.Run(async () =>
            {
                try
                {
                    var recordSorting = Builders<WithdrawalOrdersMongoEntity>.Sort.Descending(x => x.CreationUnixTime);
                    var skip = (pageNumber - 1) * limit;

                    var response = await DB.Collection<WithdrawalOrdersMongoEntity>()
                                   .Find(filter)
                                   .Sort(recordSorting)
                                   .Skip(skip)
                                   .Limit(limit)
                                   .ToListAsync();
                    resultResult.AddRange(response.Select(x => { x.ProofContent = (x.ProofContent ?? string.Empty).Length > 0 ? "1" : null; return x; }));

                }
                catch (Exception ex)
                {
                    NlogLogger.Error("GetAllWithPagination", ex);
                }
            }));

            await Task.WhenAll(taskList);

            return new WithdrawalOrderPageResultDto<WithdrawalOrdersMongoEntity>
            {
                Items = resultResult,
                TotalCount = TotalCount,
                FeeMoneyTotal = SumFeeMoney,
                OrderMoneyTotal = SumOrderMoney,
            };
        }

        private FilterDefinition<WithdrawalOrdersMongoEntity> BuildExcelFilters(GetAllWithdrawalOrdersForExcelInput input, List<int> merchantIds, List<int> deviceIds)
        {
            var orderStatus = input.OrderStatusFilter.HasValue ? (WithdrawalOrderStatusEnum)input.OrderStatusFilter
                : default;

            var notifyStatus = input.NotifyStatusFilter.HasValue ? (WithdrawalNotifyStatusEnum)input.NotifyStatusFilter
                : default;

            var orderCreationTimeStartDate = input.OrderCreationTimeStartDate;

            var orderCreationTimeEndDate = input.OrderCreationTimeStartDate;

            if (orderCreationTimeStartDate.HasValue)
            {
                orderCreationTimeStartDate = CultureTimeHelper.GetCultureTimeInfoByGTM(
                    input.OrderCreationTimeStartDate.Value, "GMT7+");
            }

            if (orderCreationTimeEndDate.HasValue)
            {
                orderCreationTimeEndDate = CultureTimeHelper.GetCultureTimeInfoByGTM(
                    input.OrderCreationTimeEndDate.Value, "GMT7+");
            }

            var builder = Builders<WithdrawalOrdersMongoEntity>.Filter;

            var userMerchantIdFilter = builder.In(e => e.MerchantId, merchantIds);

            var minCreationTimeFilter = orderCreationTimeStartDate.HasValue ? builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(orderCreationTimeStartDate.Value))
                : builder.Empty;

            var maxCreationTimeFilter = orderCreationTimeEndDate.HasValue ? builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(orderCreationTimeEndDate.Value))
                : builder.Empty;

            var orderFilter = !string.IsNullOrEmpty(input.Filter) ? builder.Text($"\"{input.Filter}\"")
                : builder.Empty;

            var merchantCodeFilter = !string.IsNullOrEmpty(input.MerchantCodeFilter) ? builder.Eq(e => e.MerchantCode, input.MerchantCodeFilter)
                : builder.Empty;

            var orderNumberFilter = !string.IsNullOrEmpty(input.OrderNoFilter) ? builder.Regex(e => e.OrderNumber, new BsonRegularExpression(input.OrderNoFilter, "i"))
                : builder.Empty;

            var orderStatusFilter = input.OrderStatusFilter.HasValue && input.OrderStatusFilter > -1 ? builder.Eq(e => e.OrderStatus, orderStatus)
                : builder.Empty;

            var notifyStatusFilter = input.NotifyStatusFilter.HasValue && input.NotifyStatusFilter > -1 ? builder.Eq(e => e.NotifyStatus, notifyStatus)
                : builder.Empty;

            var benAcctNameFilter = !string.IsNullOrEmpty(input.BenAccountNameFilter) ? builder.Eq(e => e.BenAccountName, input.BenAccountNameFilter)
                : builder.Empty;

            var benAcctNoFilter = !string.IsNullOrEmpty(input.BenAccountNoFilter)
                ? builder.Eq(e => e.BenAccountNo, input.BenAccountNoFilter)
                : builder.Empty;

            var deviceFilter = deviceIds.Count > 0
                ? builder.In(e => e.DeviceId, deviceIds)
                : builder.Empty;
            var minMoneyFilter= (input.MinMoneyFilter!=null && input.MinMoneyFilter > 0)
                ? builder.Gte(e => e.OrderMoney, input.MinMoneyFilter)
                : builder.Empty;
            var maxMoneyFilter = (input.MaxMoneyFilter != null && input.MaxMoneyFilter > 0)
                ? builder.Lte(e => e.OrderMoney, input.MaxMoneyFilter)
                : builder.Empty;


            // Combine all filters
            var combinedFilter = builder.And(
                userMerchantIdFilter,
                minCreationTimeFilter,
                maxCreationTimeFilter,
                orderFilter,
                merchantCodeFilter,
                orderNumberFilter,
                orderStatusFilter,
                notifyStatusFilter,
                benAcctNameFilter,
                benAcctNoFilter,
                deviceFilter,
                minMoneyFilter,
                maxMoneyFilter
            );

            return combinedFilter;
        }

        public async Task<int> GetTotalRecordExcelCount(GetAllWithdrawalOrdersForExcelInput input, List<int> merchantIds, List<int> deviceIds)
        {
            var filter = BuildExcelFilters(input, merchantIds, deviceIds);

            var totalRecords = await DB.Collection<WithdrawalOrdersMongoEntity>()
                .CountDocumentsAsync(filter);

            return (int)totalRecords;
        }

        public async Task<List<WithdrawalOrdersMongoEntity>> GetAll(GetAllWithdrawalOrdersForExcelInput input, List<int> merchantIds, List<int> deviceIds)
        {
            var filter = BuildExcelFilters(input, merchantIds, deviceIds);
            var response = await DB.Find<WithdrawalOrdersMongoEntity>()
                   .ProjectExcluding(pe => new { pe.ProofContent })
                    .Match(filter)
                    .Sort(s => s.CreationUnixTime, Order.Descending)
                    .ExecuteAsync();

            return response;
        }

        public async Task<bool> UpdateReceipt(WithdrawalOrdersMongoEntity newData)
        {

            var result = await DB.Update<WithdrawalOrdersMongoEntity>()
                      .MatchID(newData.ID)
                      .Modify(a => a.BinaryContentId, newData.BinaryContentId)
                      .Modify(a => a.ContentMIMEType, newData.ContentMIMEType)
                      .ExecuteAsync();

            return result.IsModifiedCountAvailable; // Must Matched With Condition 
        }

        public async Task<bool> UpdateOrderStatusAsync(WithdrawalOrdersMongoEntity order)
        {
            var result = await DB.Update<WithdrawalOrdersMongoEntity>()
                .MatchID(order.ID)
                .Modify(a => a.OrderStatus, order.OrderStatus)
                .Modify(a => a.TransactionNo, order.TransactionNo)
                .Modify(a => a.TransactionUnixTime, order.TransactionUnixTime)
                .Modify(a => a.TransactionTime, order.TransactionTime)
                .Modify(a => a.Remark, order.Remark)
                .ExecuteAsync();

            return result.IsModifiedCountAvailable; // Must Matched With Condition 
        }

        public static UpdateDefinition<T> CreatePartialUpdate<T>(T original, T modified)
        {
            var updates = new List<UpdateDefinition<T>>();
            var builder = Builders<T>.Update;

            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                var originalValue = prop.GetValue(original);
                var modifiedValue = prop.GetValue(modified);

                if (!Equals(originalValue, modifiedValue))
                {
                    updates.Add(builder.Set(prop.Name, modifiedValue));
                }
            }

            return updates.Count > 0 ? builder.Combine(updates) : null;
        }



        public async Task<List<MerchantDashboardDto>> GetAllforMerchantDashboardSummary(GetAllMerchantDashboardInput input, List<int> merchantIds)
        {

            var response = new List<MerchantDashboardDto>();

            if (input.OrderCreationTimeStartDate.HasValue)
            {
                input.OrderCreationTimeStartDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeStartDate.Value, "GMT7+");
            }
            if (input.OrderCreationTimeEndDate.HasValue)
            {
                input.OrderCreationTimeEndDate = CultureTimeHelper.GetCultureTimeInfoByGTM(input.OrderCreationTimeEndDate.Value, "GMT7+");
            }

            var builder = Builders<WithdrawalOrdersMongoEntity>.Filter;

            FilterDefinition<WithdrawalOrdersMongoEntity> userMerchantIdFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> minCreationTimeFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> maxCreationTimeFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> merchantCodeFilter = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> orderStatusFilter = builder.Empty;

            userMerchantIdFilter = builder.In(e => e.MerchantId, merchantIds);
            minCreationTimeFilter = builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(input.OrderCreationTimeStartDate.Value));
            maxCreationTimeFilter = builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp(input.OrderCreationTimeEndDate.Value));
            orderStatusFilter = builder.Eq(e => e.OrderStatus, WithdrawalOrderStatusEnum.Success);

            var filter = builder.And(
                                userMerchantIdFilter,
                                minCreationTimeFilter,
                                maxCreationTimeFilter,
                                orderStatusFilter,
                                merchantCodeFilter
                                );

            var collection = DB.Collection<WithdrawalOrdersMongoEntity>();
            var pipeline = new[] {
                new BsonDocument("$match", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<WithdrawalOrdersMongoEntity>(), BsonSerializer.SerializerRegistry)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$MerchantId" },
                    { "totalCount", new BsonDocument("$sum", 1) },
                    { "sumOrderMoney", new BsonDocument("$sum", "$OrderMoney") },
                    { "sumFeeMoney", new BsonDocument("$sum", "$FeeMoney") },
                })
            };

            var sumResult = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            if (sumResult.Count > 0)
                response = sumResult.Select(s => new MerchantDashboardDto
                {
                    MerchantId = s["_id"].ToInt64(),
                    CurrentWithdrawalOrder = s["sumOrderMoney"].AsDecimal,
                    CurrentWithdrawOrderMerchantFee = s["sumFeeMoney"].AsDecimal,
                    TotalCurrentWithdrawalOrderCount = s["totalCount"].ToInt32()
                }).ToList();

            return response;
        }

        public async Task<bool> ReleaseWithdrawal(string withdrawalId , string user)
        {
            var result = await DB.Update<WithdrawalOrdersMongoEntity>()
                   .MatchID(withdrawalId)
                   .Match(a=>a.ReleaseStatus ==  WithdrawalReleaseStatusEnum.PendingRelease)
                   .Modify(a => a.ReleaseStatus, WithdrawalReleaseStatusEnum.Released)
                   .Modify(a => a.ReleasedBy, user)
                   .Modify(a => a.ReleasedDate, DateTime.UtcNow)
                   .ExecuteAsync();

            return result.IsModifiedCountAvailable; // Must Matched With Condition 
        }

        public async Task<decimal> GetAllPendingReleaseOrder(int MerchantId)
        {
            decimal lockedAmount = 0;
            var builder = Builders<WithdrawalOrdersMongoEntity>.Filter;
            FilterDefinition<WithdrawalOrdersMongoEntity> merchantId = builder.Empty;
            FilterDefinition<WithdrawalOrdersMongoEntity> releaseStatus = builder.Empty;
            merchantId = builder.Eq(e => e.MerchantId, MerchantId);
            releaseStatus = builder.Eq(e => e.ReleaseStatus, WithdrawalReleaseStatusEnum.PendingRelease);

            var filter = builder.And(
                            merchantId,
                            releaseStatus
                            );

            var collection = DB.Collection<WithdrawalOrdersMongoEntity>();
            var pipeline = new[] {
                new BsonDocument("$match", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<WithdrawalOrdersMongoEntity>(), BsonSerializer.SerializerRegistry)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$MerchantId" },
                    { "sumOrderMoney", new BsonDocument("$sum", "$OrderMoney") },
                })
            };

            var sumResult = await collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

            if (sumResult != null)
                lockedAmount = sumResult["sumOrderMoney"].AsDecimal;

            return lockedAmount;
        }



        public void Dispose()
        {
        }
    }
}

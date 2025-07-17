using Abp.Application.Services.Dto;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.Commons;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Abp.Extensions;
using Neptune.NsPay.Common;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayOrders;
using MongoDB.Bson.Serialization;
using Neptune.NsPay.Authorization.Users;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Neptune.NsPay.MongoDbExtensions.Services
{
    public class PayOrderDepositsMongoService : MongoBaseService<PayOrderDepositsMongoEntity>, IPayOrderDepositsMongoService, IDisposable
    {
        public PayOrderDepositsMongoService() { }

        public async Task<PayOrderDepositsMongoEntity> GetPayOrderByBankNoTime(string refNo, int payMentId, string Description)
        {
            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                .ManyAsync(f => f.Eq(a => a.RefNo, refNo)
                                              & f.Eq(a => a.PayMentId, payMentId)
                                              & f.Eq(a => a.Description, Description));

            return result.FirstOrDefault();
        }

        public async Task<PayOrderDepositsMongoEntity> GetPayOrderByBankNoDesc(string refNo, int payMentId)
        {
            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                .ManyAsync(f => f.Eq(a => a.RefNo, refNo)
                                              & f.Eq(a => a.PayMentId, payMentId));

            return result.FirstOrDefault();
        }

        public async Task<List<PayOrderDepositsMongoEntity>> GetPayOrdersByBankNosDesc(List<string> refNos, int payMentId)
        {
            var filter = Builders<PayOrderDepositsMongoEntity>.Filter.And(
                    Builders<PayOrderDepositsMongoEntity>.Filter.In(entity => entity.RefNo, refNos),
                    Builders<PayOrderDepositsMongoEntity>.Filter.Eq(entity => entity.PayMentId, payMentId)
            );

            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                     .Match(filter)
                                     .ExecuteAsync();
            return result.ToList();
        }

        public async Task<List<PayOrderDepositsMongoEntity>> GetPayOrderByBankNosTime(List<string> refNos, int payMentId, List<string> Description)
        {
            var filter = Builders<PayOrderDepositsMongoEntity>.Filter.And(
                    Builders<PayOrderDepositsMongoEntity>.Filter.In(entity => entity.RefNo, refNos),
                    Builders<PayOrderDepositsMongoEntity>.Filter.Eq(entity => entity.PayMentId, payMentId),
                      Builders<PayOrderDepositsMongoEntity>.Filter.In(entity => entity.Description, Description)
            );

            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                   .Match(filter)
                                   .ExecuteAsync();
            return result.ToList();
        }


        public async Task<PayOrderDepositsMongoEntity> GetPayOrderByBankInDesc(string description, int payMentId)
        {
            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                .ManyAsync(f => f.Eq(a => a.Description, description)
                                              & f.Eq(a => a.PayMentId, payMentId));

            return result.FirstOrDefault();
        }



        public async Task<PayOrderDepositsMongoEntity> GetPayOrderFirstAsync(string refNo, string description, int paymemtId)
        {
            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                .ManyAsync(f => f.Eq(a => a.RefNo, refNo)
                                              & f.Eq(a => a.Description, description)
                                              & f.Eq(a => a.PayMentId, paymemtId));

            return result.FirstOrDefault();
        }

        public async Task<PayOrderDepositsMongoEntity> GetPayOrderByBank(DateTime dateNow, string refNo, int paymemtId)
        {
            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                    .ManyAsync(f => f.Gt(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(dateNow))
                                                    & f.Eq(a => a.RefNo, refNo)
                                                    & f.Eq(a => a.PayMentId, paymemtId)
                                                );

            return result.FirstOrDefault();
        }

        public async Task<PayOrderDepositsMongoEntity> GetPayOrderByBank(DateTime dateNow, string refNo, int paymemtId, string orderMark)
        {
            //var result = await DB.Find<PayOrderDepositsMongoEntity>()
            //                        .ManyAsync(f => f.Gt(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(dateNow))
            //                                        & f.Eq(a => a.RefNo, refNo)
            //                                        & f.Regex(a => a.Description, new MongoDB.Bson.BsonRegularExpression(desc))
            //                                        & f.Eq(a => a.PayMentId, paymemtId)
            //                                    );
            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                    .ManyAsync(f => f.Gt(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(dateNow))
                                                    & f.Eq(a => a.RefNo, refNo)
                                                    //& f.Regex(a => a.Description, new MongoDB.Bson.BsonRegularExpression(orderMark))
                                                    & f.Eq(a => a.PayMentId, paymemtId)
                                                );
            var info = result.FirstOrDefault(r => r.Description.Trim().Replace(" ", "").Contains(orderMark.Trim().Replace(" ", "")));
            return info;
        }

        public async Task<List<PayOrderDepositsMongoEntity>> GetPayOrderDepositByDateRange(DateTime startDate, DateTime endDate)
        {
            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                .ManyAsync(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                              & f.Lt(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate)));
            return result.ToList();
        }

        public async Task<List<PayOrderDepositsMongoEntity>> GetPayOrderDepositByPaymentIdDateRange(DateTime startDate, DateTime endDate, List<int> payMentId)
        {
            if (payMentId.Count == 1)
            {
                var id = payMentId[0];
                var result = await DB.Find<PayOrderDepositsMongoEntity>()
                    .Match(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                  & f.Lt(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate))
                                  & f.Eq(a => a.PayMentId, id))
                    .Project(a => a.Include("Type").Include("OrderId").Include("UserId").Include("CreditAmount").Include("DebitAmount").Include("PayMentId"))
                    .ExecuteCursorAsync();
                return result.ToList();
            }
            else
            {
                var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                    .Match(f => f.Gte(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(startDate))
                                                  & f.Lt(a => a.CreationUnixTime, TimeHelper.GetUnixTimeStamp(endDate))
                                                  & f.In(a => a.PayMentId, payMentId))
                                    .Project(a => a.Include("Type").Include("OrderId").Include("UserId").Include("CreditAmount").Include("DebitAmount").Include("PayMentId"))
                                    .ExecuteCursorAsync();
                return result.ToList();
            }
        }


        public async Task<PayOrderDepositsMongoEntity?> GetPayOrderDepositsByBankOrderId(string orderId, PayMentTypeEnum payType)
        {
            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                    .ManyAsync(f => f.Eq(a => a.ID, orderId)
                                                    & f.Eq(a => a.PayType, (int)payType)
                                                    & f.Gt(a => a.CreditAmount, 0)
                                                    & f.Eq(a => a.Type, "CRDT")
                                                );
            return result.FirstOrDefault();
        }

        public async Task<PagedResultDto<PayOrderDepositsMongoEntity>> GetAllWithPagination(GetAllPayOrderDepositsInput input, List<int> payMentIds, List<string> orders, List<string> payOrderUserNos)
        {
            if (input.MaxTransactionTimeFilter.HasValue)
            {
                input.MaxTransactionTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MaxTransactionTimeFilter.Value, input.UtcTimeFilter);
            }
            if (input.MinTransactionTimeFilter.HasValue)
            {
                input.MinTransactionTimeFilter = CultureTimeHelper.GetCultureTimeInfoByGTM(input.MinTransactionTimeFilter.Value, input.UtcTimeFilter);
            }

            var builder = Builders<PayOrderDepositsMongoEntity>.Filter;

            FilterDefinition<PayOrderDepositsMongoEntity> orderFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> payMentIdsFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> minCreationTimeFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> maxCreationTimeFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> accountNoFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> userNameFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> merchantCodeFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> merchantIdFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> depositOrderStatusFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> orderNoFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> orderMarkFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> userMemberFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> minMoneyFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> maxMoneyFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> orderPayTypeFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> bankOrderStatusFilter = builder.Empty;
            FilterDefinition<PayOrderDepositsMongoEntity> refNoFilter = builder.Empty;

            if (!input.Filter.IsNullOrEmpty())
            {
                //orderFilter = builder.Text(input.Filter);
                var textfilter = "\"" + input.Filter + "\"";
                orderFilter = builder.Text(textfilter);
            }

            if (input.MerchantIds != null)
            {
                if (input.MerchantIds.Count == 0)
                {
                    merchantIdFilter = builder.Eq(e => e.MerchantId, input.MerchantIds.FirstOrDefault());
                }
                else
                {
                    merchantIdFilter = builder.In(e => e.MerchantId, input.MerchantIds);
                }
            }

            if (payMentIds.Count == 1)
                payMentIdsFilter = builder.Eq(e => e.PayMentId, payMentIds.FirstOrDefault());
            else
                payMentIdsFilter = builder.In(e => e.PayMentId, payMentIds.Distinct().OrderBy(x => x));

            if (input.MinTransactionTimeFilter != null)
                minCreationTimeFilter = builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MinTransactionTimeFilter));

            if (input.MaxTransactionTimeFilter != null)
                maxCreationTimeFilter = builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)input.MaxTransactionTimeFilter));

            if (!input.AccountNoFilter.IsNullOrEmpty())
                accountNoFilter = builder.Eq(e => e.AccountNo, input.AccountNoFilter);

            if (!input.UserNameFilter.IsNullOrEmpty())
                userNameFilter = builder.Eq(e => e.UserName, input.UserNameFilter);

            if (!input.OrderMarkFilter.IsNullOrEmpty())
                orderMarkFilter = builder.Regex(e => e.Description, input.OrderMarkFilter);

            if (!input.MerchantCodeFilter.IsNullOrEmpty())
                merchantCodeFilter = builder.Eq(e => e.MerchantCode, input.MerchantCodeFilter);

            if (input.DepositOrderStatusFilter == 1)
                depositOrderStatusFilter = builder.And(
                        builder.Ne(r => r.OrderId, null),
                        builder.Ne(r => r.OrderId, "-1"),
                        builder.Eq(e => e.UserId, 0)
                    );

            if (input.DepositOrderStatusFilter == 2)
                depositOrderStatusFilter = builder.Eq(e => e.OrderId, null);

            if (input.DepositOrderStatusFilter == 3)
                depositOrderStatusFilter = builder.Eq(e => e.OrderId, "-1");

            if (input.DepositOrderStatusFilter == 4)
            {
                depositOrderStatusFilter = builder.And(
                                    builder.Ne(r => r.OrderId, null),
                                    builder.Ne(r => r.OrderId, "-1"),
                                    builder.Gt(e => e.UserId, 0)
                                );
            }

            if (!input.OrderNoFilter.IsNullOrEmpty())
            {
                if (orders.Count > 0)
                {
                    if (orders.Count == 1)
                    {
                        orderNoFilter = builder.Eq(e => e.OrderId, orders.FirstOrDefault());
                    }
                    else
                    {
                        orderNoFilter = builder.In(e => e.OrderId, orders);
                    }
                }
                else
                {
                    orderNoFilter = builder.Eq(e => e.OrderId, "NoOrder");
                }
            }

            if (!input.UserMemberFilter.IsNullOrEmpty())
            {
                if (payOrderUserNos.Count > 0)
                {
                    if (payOrderUserNos.Count == 1)
                    {
                        userMemberFilter = builder.Eq(e => e.OrderId, payOrderUserNos.FirstOrDefault());
                    }
                    else
                    {
                        userMemberFilter = builder.In(e => e.OrderId, payOrderUserNos);
                    }
                }
                else
                {
                    userMemberFilter = builder.Eq(e => e.OrderId, "NoOrder");
                }
            }

            if (input.MinMoneyFilter != null)
                minMoneyFilter = builder.Gte(e => e.CreditAmount, input.MinMoneyFilter);

            if (input.MaxMoneyFilter != null)
                maxMoneyFilter = builder.Lte(e => e.CreditAmount, input.MaxMoneyFilter);

            if (input.OrderPayTypeFilter > 0)
                orderPayTypeFilter = builder.Eq(e => e.PayType, input.OrderPayTypeFilter);

            if (!input.BankOrderStatusFilter.IsNullOrEmpty())
            {
                if (input.BankOrderStatusFilter != "-1")
                {
                    bankOrderStatusFilter = builder.Eq(e => e.Type, input.BankOrderStatusFilter);
                }
            }

            if (!input.RefNoFilter.IsNullOrEmpty())
            {
                refNoFilter = builder.Eq(e => e.RefNo, input.RefNoFilter);
            }

            var filter = builder.And(
                                payMentIdsFilter,
                                minCreationTimeFilter,
                                maxCreationTimeFilter,
                                merchantIdFilter,
                                merchantCodeFilter,
                                orderPayTypeFilter,
                                orderFilter,
                                orderMarkFilter,
                                accountNoFilter,
                                userNameFilter,
                                depositOrderStatusFilter,
                                orderNoFilter,
                                userMemberFilter,
                                minMoneyFilter,
                                maxMoneyFilter,
                                bankOrderStatusFilter,
                                refNoFilter
                                );

            int skip = input.SkipCount;
            int limit = input.MaxResultCount;
            int pageNumber = (skip / limit) + 1;
            int totalCount = 0;

            List<Task> taskList = new List<Task>();

            taskList.Add(Task.Run(async () =>
            {
                var pipeline = new[] {
                new BsonDocument("$match", filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<PayOrderDepositsMongoEntity>(), BsonSerializer.SerializerRegistry)),

                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "totalCount", new BsonDocument("$sum", 1) },
                })
            };

                var sumResult = await DB.Collection<PayOrderDepositsMongoEntity>().Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

                if (sumResult != null)
                {
                    totalCount = sumResult["totalCount"].ToInt32();
                }

            }));

            List<PayOrderDepositsMongoEntity> result = new List<PayOrderDepositsMongoEntity>();
            taskList.Add(Task.Run(async () =>
            {
                var recordSorting = Builders<PayOrderDepositsMongoEntity>.Sort.Descending(x => x.CreationUnixTime);
                var skip = (pageNumber - 1) * limit;

                var response = await DB.Collection<PayOrderDepositsMongoEntity>()
                               .Find(filter)
                               .Sort(recordSorting)
                               .Skip(skip)
                               .Limit(limit)
                               .ToListAsync();

                result.AddRange(response);
            }));

            await Task.WhenAll(taskList);

            return new PagedResultDto<PayOrderDepositsMongoEntity>
            {
                Items = result,
                TotalCount = totalCount
            };
        }

        public async Task<List<PayOrderDepositsMongoEntity>> GetAll(GetAllPayOrderDepositsForExcelInput input, List<int> payMentIds, List<string> orders, List<string> payOrderUserNos)
        {
            var filter = BuildExcelFilters(input, payMentIds, orders, payOrderUserNos);

            var response = await DB.Find<PayOrderDepositsMongoEntity>()
                    .Match(filter)
                    .Sort(s => s.CreationUnixTime, MongoDB.Entities.Order.Descending)
                    .ExecuteAsync();

            return response;
        }


        public async Task<int> GetTotalExcelRecordCount(GetAllPayOrderDepositsForExcelInput input, List<int> payMentIds, List<string> orders, List<string> payOrderUserNos)
        {
            var filter = BuildExcelFilters(input, payMentIds, orders, payOrderUserNos);

            var totalRecords = await DB.Collection<PayOrderDepositsMongoEntity>()
                .CountDocumentsAsync(filter);

            return (int)totalRecords;
        }


        private FilterDefinition<PayOrderDepositsMongoEntity> BuildExcelFilters(GetAllPayOrderDepositsForExcelInput input, List<int> payMentIds, List<string> orders, List<string> payOrderUserNos)
        {
            bool IsNotNullOrEmpty(string input) => !string.IsNullOrEmpty(input);

            void AddFilter(ref List<FilterDefinition<PayOrderDepositsMongoEntity>> filters, FilterDefinition<PayOrderDepositsMongoEntity> filter)
            {
                if (filter != Builders<PayOrderDepositsMongoEntity>.Filter.Empty)
                    filters.Add(filter);
            }

            DateTime? maxTransactionTimeFilter = input.MaxTransactionTimeFilter.HasValue
                   ? CultureTimeHelper.GetCultureTimeInfoByGTM(input.MaxTransactionTimeFilter.Value, input.UtcTimeFilter)
                   : (DateTime?)null;

            DateTime? minTransactionTimeFilter = input.MinTransactionTimeFilter.HasValue
                ? CultureTimeHelper.GetCultureTimeInfoByGTM(input.MinTransactionTimeFilter.Value, input.UtcTimeFilter)
                : (DateTime?)null;

            var builder = Builders<PayOrderDepositsMongoEntity>.Filter;
            var filters = new List<FilterDefinition<PayOrderDepositsMongoEntity>>();

            if (IsNotNullOrEmpty(input.Filter))
                AddFilter(ref filters, builder.Text(input.Filter));

            if (payMentIds.Any())
                AddFilter(ref filters, payMentIds.Count == 1 ? builder.Eq(e => e.PayMentId, payMentIds.First()) : builder.In(e => e.PayMentId, payMentIds));

            if (minTransactionTimeFilter.HasValue)
                AddFilter(ref filters, builder.Gte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)minTransactionTimeFilter)));

            if (maxTransactionTimeFilter.HasValue)
                AddFilter(ref filters, builder.Lte(e => e.CreationUnixTime, TimeHelper.GetUnixTimeStamp((DateTime)maxTransactionTimeFilter)));

            if (IsNotNullOrEmpty(input.AccountNoFilter))
                AddFilter(ref filters, builder.Eq(e => e.AccountNo, input.AccountNoFilter));

            if (IsNotNullOrEmpty(input.OrderMarkFilter))
                AddFilter(ref filters, builder.Regex(e => e.Description, input.OrderMarkFilter));

            if (IsNotNullOrEmpty(input.UserNameFilter))
                AddFilter(ref filters, builder.Eq(e => e.UserName, input.UserNameFilter));

            if (IsNotNullOrEmpty(input.MerchantCodeFilter))
                AddFilter(ref filters, builder.Eq(e => e.MerchantCode, input.MerchantCodeFilter));

            if (input.DepositOrderStatusFilter == 1)
                AddFilter(ref filters, builder.And(
                    builder.Ne(r => r.OrderId, null),
                    builder.Ne(r => r.OrderId, "-1"),
                    builder.Eq(e => e.UserId, 0)));

            if (input.DepositOrderStatusFilter == 2)
                AddFilter(ref filters, builder.Eq(e => e.OrderId, null));

            if (input.DepositOrderStatusFilter == 3)
                AddFilter(ref filters, builder.Eq(e => e.OrderId, "-1"));

            if (input.DepositOrderStatusFilter == 4)
                AddFilter(ref filters, builder.And(
                    builder.Ne(r => r.OrderId, null),
                    builder.Ne(r => r.OrderId, "-1"),
                    builder.Gt(e => e.UserId, 0)));

            if (IsNotNullOrEmpty(input.OrderNoFilter))
            {
                if (orders.Any())
                    AddFilter(ref filters, orders.Count == 1 ? builder.Eq(e => e.OrderId, orders.First()) : builder.In(e => e.OrderId, orders));
                else
                    AddFilter(ref filters, builder.Eq(e => e.OrderId, "NoOrder"));
            }

            if (IsNotNullOrEmpty(input.UserMemberFilter))
            {
                if (payOrderUserNos.Any())
                    AddFilter(ref filters, payOrderUserNos.Count == 1 ? builder.Eq(e => e.OrderId, payOrderUserNos.First()) : builder.In(e => e.OrderId, payOrderUserNos));
                else
                    AddFilter(ref filters, builder.Eq(e => e.OrderId, "NoOrder"));
            }

            if (input.MinMoneyFilter.HasValue)
                AddFilter(ref filters, builder.Gte(e => e.CreditAmount, input.MinMoneyFilter));

            if (input.MaxMoneyFilter.HasValue)
                AddFilter(ref filters, builder.Lte(e => e.CreditAmount, input.MaxMoneyFilter));

            if (input.OrderPayTypeFilter > 0)
                AddFilter(ref filters, builder.Eq(e => e.PayType, input.OrderPayTypeFilter));

            if (IsNotNullOrEmpty(input.BankOrderStatusFilter) && input.BankOrderStatusFilter != "-1")
                AddFilter(ref filters, builder.Eq(e => e.Type, input.BankOrderStatusFilter));

            return filters.Any() ? builder.And(filters) : builder.Empty;
        }


        public async Task<PayOrderDepositsMongoEntity> GetPayOrderDepositsByBankOrderIdNoType(string orderId, PayMentTypeEnum payType)
        {
            var result = await DB.Find<PayOrderDepositsMongoEntity>()
                                .ManyAsync(f => f.Eq(a => a.ID, orderId)
                                                & f.Eq(a => a.PayType, (int)payType)
                                            );
            return result.FirstOrDefault();

        }

        public async Task PayOrderDepositAssociated(string merchantCode, string payOrderId, decimal orderMoney, string transactionNo, int payMentId, PayMentTypeEnum payType, string remark, string bankId, int merchanrId, long userId)
        {
            try
            {
                using (var TN = DB.Transaction())
                {
                    var datetime = DateTime.Now;
                    //更新订单表
                    await TN.Update<PayOrdersMongoEntity>()
                        .Match(r => r.MerchantCode == merchantCode && r.ID == payOrderId)
                        .Modify(r => r.OrderType, PayOrderOrderTypeEnum.Justify)
                        .Modify(r => r.OrderStatus, PayOrderOrderStatusEnum.Completed)
                        .Modify(r => r.TransactionNo, transactionNo)
                        .Modify(r => r.TransactionTime, datetime)
                        .Modify(r => r.TransactionUnixTime, TimeHelper.GetUnixTimeStamp(datetime))
                        .Modify(r => r.ScoreStatus, PayOrderScoreStatusEnum.NoScore)
                        .Modify(r => r.ScoreNumber, 0)
                        .Modify(r => r.TradeMoney, orderMoney)
                        .Modify(r => r.PayMentId, payMentId)
                        .Modify(r => r.PayType, payType)
                        .Modify(r => r.Remark, remark)
                        .ExecuteAsync();

                    //更新银行订单表
                    await TN.Update<PayOrderDepositsMongoEntity>()
                        .MatchID(bankId)
                        .Modify(r => r.OrderId, payOrderId)
                        .Modify(r => r.MerchantCode, merchantCode)
                        .Modify(r => r.MerchantId, merchanrId)
                        .Modify(r => r.UserId, userId)
                        .Modify(r => r.OperateTime, DateTime.Now)
                        .ExecuteAsync();

                    await TN.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("回调订单异常:" + ex.ToString());
            }
        }

        public async Task PayOrderDepositReject(string bankId, long userId, string rejectRemark)
        {
            await DB.Update<PayOrderDepositsMongoEntity>()
                    .MatchID(bankId)
                    .Modify(r => r.OrderId, "-1")
                    .Modify(r => r.UserId, userId)
                    .Modify(r => r.RejectRemark, rejectRemark)
                    .Modify(r => r.OperateTime, DateTime.Now)
                    .ExecuteAsync();
        }

        public async Task<bool> PayOrderDepositSubRedisUpdate(string orderId, string transactionNo, decimal orderMoney, string remark, string bankId, string merchantCode, int merchantId, int payMentId, PayMentTypeEnum payType)
        {
            try
            {
                using (var TN = DB.Transaction())
                {
                    var transferTime = DateTime.Now;
                    var transferUnixTime = TimeHelper.GetUnixTimeStamp(transferTime);
                    //更新订单表
                    await TN.Update<PayOrdersMongoEntity>()
                       .MatchID(orderId)
                       .Modify(r => r.TransactionNo, transactionNo)
                       .Modify(r => r.OrderStatus, PayOrderOrderStatusEnum.Completed)
                       .Modify(r => r.TransactionTime, transferTime)
                       .Modify(r => r.TransactionUnixTime, transferUnixTime)
                       .Modify(r => r.TradeMoney, orderMoney)
                       .Modify(r => r.PayMentId, payMentId)
                       .Modify(r => r.PayType, payType)
                       .Modify(r => r.Remark, remark)
                       .ExecuteAsync();

                    //更新银行订单表
                    await TN.Update<PayOrderDepositsMongoEntity>()
                        .MatchID(bankId)
                        .Modify(r => r.OrderId, orderId)
                        .Modify(r => r.MerchantCode, merchantCode)
                        .Modify(r => r.MerchantId, merchantId)
                        .ExecuteAsync();

                    await TN.CommitAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error("BankSubRedis更新订单异常:" + ex.ToString());
                return false;
            }
        }

        public void Dispose()
        {
        }
    }
}

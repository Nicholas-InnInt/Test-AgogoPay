using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Entities;
using Neptune.NsPay.Commons;

namespace Neptune.NsPay.MongoDbExtensions
{
    public static class SetupMongo
    {
        public static async void AddMongoSetup(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            var config = AppSettings.Configuration;

            var setting = MongoClientSettings.FromConnectionString(config["MongoDb:MongoDBConnection"]);
            setting.MaxConnectionPoolSize = 500;
            await DB.InitAsync(config["MongoDb:MongoDBDatabaseName"], setting);
            //await DB.InitAsync("DatabaseName", new MongoClientSettings()
            //{
            //    Server = new MongoServerAddress(config["MongoDb:MongoDBConnection"]),
            //    MinConnectionPoolSize = 100,
            //    MaxConnectionPoolSize = 500
            //});
        }

        //private static DataChangeType convertToChangeTypeEnum(ChangeStreamOperationType rawType)
        //{
        //    Dictionary<ChangeStreamOperationType, DataChangeType> typeMapping = new Dictionary<ChangeStreamOperationType, DataChangeType>()
        //    {{ChangeStreamOperationType.Insert , DataChangeType.Add},
        //    {ChangeStreamOperationType.Update , DataChangeType.Update},
        //    {ChangeStreamOperationType.Delete , DataChangeType.Delete},

        //    };

        //    return typeMapping.ContainsKey(rawType) ? typeMapping[rawType] : DataChangeType.Update;
        //}
        //public static async void AddWatcher(this IServiceProvider serviceProvider)
        //{
        //    if (serviceProvider == null)
        //    {
        //        throw new ArgumentNullException(nameof(serviceProvider));
        //    }
        //    var configuration = new MapperConfiguration(cfg =>
        //    {
        //        cfg.CreateMap<PayOrdersMongoEntity, PayOrdersMongoEntityGlobal>().ReverseMap();
        //    });

        //    IMapper mapper = configuration.CreateMapper();

        //    var dataEvent = serviceProvider.GetService<IDataChangedEvent>(); // returns null if not found

        //    if (dataEvent != null)
        //    {
        //        // var watcher = DB.Watcher<PayOrdersMongoEntity>("PayOrders");
        //        var watcher = DB.Watcher<PayOrdersMongoEntity>("PayOrders");


        //        watcher.OnChangesCSD += changes =>
        //        {
        //            foreach (var csd in changes)
        //            {
        //                Console.WriteLine("Changes -");

        //                dataEvent.MongoDbchange<PayOrdersMongoEntityGlobal>(mapper.Map<PayOrdersMongoEntityGlobal>(csd.FullDocument), convertToChangeTypeEnum(csd.OperationType));
        //            }

        //        };


        //        watcher.Start(
        //            eventTypes: EventType.Created | EventType.Updated | EventType.Deleted,
        //            batchSize: 20);

        //    }
        //}
    }
}

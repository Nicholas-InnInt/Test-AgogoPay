using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neptune.NsPay.Commons;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
    public class BaseMongoEntity : Entity
    {
        public long CreationUnixTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreationTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TransactionTime { get; set; }
        public long TransactionUnixTime { get; set; }

        public BaseMongoEntity() 
        {
            CreationTime = DateTime.Now;
            CreationUnixTime = TimeHelper.GetUnixTimeStamp(CreationTime);

            TransactionTime = DateTime.Now;
            TransactionUnixTime = TimeHelper.GetUnixTimeStamp(TransactionTime);
        }
    }
}

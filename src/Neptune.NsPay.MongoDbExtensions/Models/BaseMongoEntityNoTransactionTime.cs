using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neptune.NsPay.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.MongoDbExtensions.Models
{
    public class BaseMongoEntityNoTransactionTime: Entity
    {
        public long CreationUnixTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreationTime { get; set; }

        public BaseMongoEntityNoTransactionTime()
        {
            CreationTime = DateTime.Now;
            CreationUnixTime = TimeHelper.GetUnixTimeStamp(CreationTime);
        }
    }
}

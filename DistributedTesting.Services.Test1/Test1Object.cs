using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedTesting.Services.Test1
{
    public class Test1Object
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("String1")]
        public string String1 { get; set; }

        [BsonElement("Int1")]
        public int Int1 { get; set; }
    }
}

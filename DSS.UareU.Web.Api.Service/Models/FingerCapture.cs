using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Models
{
    public class FingerCapture
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string AccountId { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<string, string> Tags { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Binary)]
        public byte[] Image { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Binary)]
        public byte[] WSQImage { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Binary)]
        public byte[] FMD { get; set; }

        public static IMongoCollection<FingerCapture> GetCollection()
        {
            return DBClient.GetCollection<FingerCapture>("fingerCapture");
        }
    }
}

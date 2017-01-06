using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service
{
    public static class DBClient
    {
        public static IMongoClient Instance { get; set; }
        public static IMongoDatabase Database { get; set; }

        public static IMongoCollection<T> GetCollection<T>(string name)
        {
            if (DBClient.Instance != null)
            {
                return DBClient.Database.GetCollection<T>(name);
            } else
            {
                throw new Exception("DB not initialized");
            }
        }
    }
}

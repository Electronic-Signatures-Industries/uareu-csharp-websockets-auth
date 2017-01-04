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
        public static MongoClient Instance { get; set; }
    }
}

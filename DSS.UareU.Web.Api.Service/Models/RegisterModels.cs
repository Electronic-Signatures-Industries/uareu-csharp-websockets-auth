using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Models
{
    public class RegisterModels
    {
        public static void Bind()
        {
            BsonClassMap.RegisterClassMap<FingerCapture>();
        }
    }
}

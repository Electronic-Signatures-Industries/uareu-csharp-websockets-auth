using DSS.UareU.Web.Api.Service.Models;
using MongoDB.Driver;
using Nancy.Bootstrapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service
{
    public class ApiBootstrap : IApplicationStartup
    {
        public void Initialize(IPipelines pipelines)
        {
            var cs = ConfigurationManager.AppSettings["DB"];
            RegisterModels.Bind();
            DBClient.Instance = new MongoClient(cs);
        }
    }
}

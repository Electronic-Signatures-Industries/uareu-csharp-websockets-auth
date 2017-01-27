using DPUruNet;
using DSS.UareU.Web.Api.Client.Services;
using Nancy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Controllers
{
    public class ApiModule : NancyModule
    {
        public ApiModule() : base("/api")
        {
            Get["/health"] = parameters =>
            {
                return "OK";
            };

            Get["/v1/device_info", true] = async (parameters, ct) =>
            {
                ReaderService service = new ReaderService();
                return await service.GetReaderInfo();
            };

        }
    }
}

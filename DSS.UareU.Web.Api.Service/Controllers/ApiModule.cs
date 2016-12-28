using DPUruNet;
using DSS.UareU.Web.Api.Service.Services;
using Nancy;
using System;
using System.Collections.Generic;
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

            Get["/info", true] = async (parameters, ct) =>
            {
                DPUareUReaderService service = new DPUareUReaderService();
                return await service.GetReaderInfo();
            };

        }
    }
}

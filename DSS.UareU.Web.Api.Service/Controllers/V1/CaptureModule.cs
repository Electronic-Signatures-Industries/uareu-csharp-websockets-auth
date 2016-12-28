using DSS.UareU.Web.Api.Service.Services;
using Nancy;
using Nancy.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Controllers.V1
{
    public class CaptureModule : NancyModule
    {
        public CaptureModule() : base("/api/v1/capture")
        {
            Get["/", true] = async (parameters, ct) =>
            {
                JsonSettings.MaxJsonLength = 50 * 10000;
                DPUareUReaderService service = new DPUareUReaderService();
                return await service.Capture();
            };

        }
    }
}

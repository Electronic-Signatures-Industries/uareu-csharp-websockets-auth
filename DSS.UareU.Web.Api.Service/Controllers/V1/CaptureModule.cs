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

        const int TIMEOUT_SECONDS = 15;
        ReaderService service = new ReaderService();

        public CaptureModule() : base("/api/v1/capture")
        {
            Get["/{id}", true] = async (parameters, ct) =>
            {
                var imageTask = service.GetCaptureImageAsync(parameters.id);
                return await imageTask;
            };

            Post["/", true] = async (parameters, ct) =>
            {                
                JsonSettings.MaxJsonLength = 50 * 10000;
                var captureTask = service.CaptureAsync();

                if (captureTask == await Task.WhenAny(captureTask, Task.Delay(TIMEOUT_SECONDS * 1000))) {
                    return await captureTask;
                } else {
                    service.Close();
                    return Task.FromResult(new { Message = "Timeout after 15 seconds" });
                }
            };

        }
    }
}


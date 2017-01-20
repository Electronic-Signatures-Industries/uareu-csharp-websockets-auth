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
            Put["/{id}", true] = async (parameters, ct) =>
            {
                return await service.UpdateCaptureModel(null);
            };

            Get["/{id}", true] = async (parameters, ct) =>
            {

                bool wsq = false;

                if (Request.Query["wsq"] != null)
                {
                    wsq = true;
                }
                var imageTask = service.GetCaptureImageAsync(parameters.id, wsq);
                return await imageTask;
            };

            Post["/", true] = async (parameters, ct) =>
            {                
                JsonSettings.MaxJsonLength = 50 * 10000;
                bool temporary = false;

                if (Request.Query["temporary"] != null)
                {
                    temporary = true;
                }

                var captureTask = service.CaptureAsync(temporary);

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


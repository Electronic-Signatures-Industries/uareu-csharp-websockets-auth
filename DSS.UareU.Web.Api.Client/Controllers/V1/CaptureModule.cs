using Nancy;
using Nancy.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Security;
using DSS.UareU.Web.Api.Client.Services;
using DSS.UareU.Web.Api.Shared;

namespace DSS.UareU.Web.Api.Client.Controllers.V1
{
    public class CaptureModule : NancyModule
    {

        const int TIMEOUT_SECONDS = 15;
        ReaderService service = new ReaderService();

        public CaptureModule() : base("/api/v1/capture")
        {

            Get["/{id}", true] = async (parameters, ct) =>
            {
                this.RequiresAuthentication();
                bool extended = false;

                if (Request.Query["extended"] != null)
                {
                    extended = true;
                }
                
                var imageTask = service.GetCaptureImageAsync(parameters.id, new Shared.Mediatypes.FindCaptureOptions
                {
                    Extended = extended,
                });
                return await imageTask;
            };

            Post["/", true] = async (parameters, ct) =>
            {
                this.RequiresAuthentication();


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


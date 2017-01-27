using DPUruNet;
using DSS.UareU.Web.Api.Shared.Mediatypes;
using DSS.UareU.Web.Api.Service.Services;
using Nancy;
using Nancy.Json;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Security;
using DSS.UareU.Web.Api.Shared;

namespace DSS.UareU.Web.Api.Service.Controllers
{
    public class CaptureModule : NancyModule
    {
        const int TIMEOUT_SECONDS = 30;
        ComparisonService service = new ComparisonService();

        public CaptureModule() : base("/api/v1/capture")
        {

            const int TIMEOUT_SECONDS = 15;
            CaptureService service = new CaptureService();

            Delete["/{id}"] = (parameters) =>
            {
                this.RequiresAuthentication();
                service.RemoveCapture(parameters.id);
                return ResponseMessageBuilder.BuildMessageResponse(HttpStatusCode.OK, String.Empty);
            };

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
                var request = this.Bind<CaptureRequestMediaType>();
                // JsonSettings.MaxJsonLength = 50 * 10000;

                try
                {
                    var fmd = Convert.FromBase64String(request.Fmd);
                    var wsq = Convert.FromBase64String(request.Wsq);
                    var insertTask = service.SaveCaptureAsync(fmd, wsq);

                    if (insertTask == await Task.WhenAny(insertTask, Task.Delay(TIMEOUT_SECONDS * 1000)))
                    {
                        return await insertTask;
                    }
                    else
                    {
                        var message = "Timeout after 10 seconds";
                        return Task.FromResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, message));
                    }
                }
                catch (Exception e)
                {
                    return Task.FromResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, e.Message));
                }
            };

        }
    }
}

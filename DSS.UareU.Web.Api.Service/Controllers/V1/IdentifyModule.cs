using DSS.UareU.Web.Api.Service.Mediatypes;
using DSS.UareU.Web.Api.Service.Services;
using Nancy;
using Nancy.Json;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Controllers.V1
{
    public class IdentifyModule : NancyModule
    {
        const int TIMEOUT_SECONDS = 10;
        ComparisonService service = new ComparisonService();

        public IdentifyModule() : base("/api/v1/comparison")
        {
            Post["/identify", true] = async (parameters, ct) =>
            {
                var request = this.Bind<ComparisonRequestMediaType>();
                // JsonSettings.MaxJsonLength = 50 * 10000;
                var verifyTask = service.IdentifyAsync(request.CaptureId, request.EnrolledIds);

                if (verifyTask == await Task.WhenAny(verifyTask, Task.Delay(TIMEOUT_SECONDS * 1000))) {
                    return await verifyTask;
                } else {
                    return Task.FromResult(new { Message = "Timeout after 10 seconds" });
                }
            };

        }
        
    }
}

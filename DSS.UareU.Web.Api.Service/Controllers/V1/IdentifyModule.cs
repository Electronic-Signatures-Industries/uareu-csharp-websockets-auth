using DSS.UareU.Web.Api.Service.Services;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Text;
using System.Threading.Tasks;
using Nancy.Security;
using DSS.A2F.Fingerprint.Api.Shared.Mediatypes;

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
                this.RequiresAuthentication();
                var request = this.Bind<ComparisonRequestMediaType>();
                // JsonSettings.MaxJsonLength = 50 * 10000;
                var verifyTask = service.IdentifyAsync(request.CaptureId, request.EnrolledIds);

                try
                {
                    if (verifyTask == await Task.WhenAny(verifyTask, Task.Delay(TIMEOUT_SECONDS * 1000)))
                    {
                        return await verifyTask;
                    }
                    else
                    {
                        var message = "Timeout after 10 seconds";
                        return Task.FromResult(BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, message));
                    }
                }
                catch (Exception e)
                {
                    return Task.FromResult(BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, e.Message));
                }
            };


        }

        public Nancy.Response BuildMessageResponse(Nancy.HttpStatusCode code, string message)
        {
            var m = Encoding.UTF8.GetBytes(message);
            return new Nancy.Response
            {
                StatusCode = code,
                Contents = a => a.Write(m, 0, m.Length),
            };
        }
    }
}

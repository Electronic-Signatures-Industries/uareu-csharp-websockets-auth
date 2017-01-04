using DPUruNet;
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

namespace DSS.UareU.Web.Api.Service.Controllers
{
    public class EnrollmentModule : NancyModule
    {
        const int TIMEOUT_SECONDS = 30;
        VerificationService service = new VerificationService();

        public EnrollmentModule() : base("/api")
        {
            Post["/enroll/user", true] = async (parameters, ct) =>
            {
                var verifyReqPayload = this.Bind<VerificationRequestMediaType>();

                JsonSettings.MaxJsonLength = 50 * 10000;
                var verifyTask = service.VerifyAsync(verifyReqPayload.CaptureId, verifyReqPayload.EnrolledIds);

                if (verifyTask == await Task.WhenAny(verifyTask, Task.Delay(TIMEOUT_SECONDS * 1000)))
                {
                    return await verifyTask;
                }
                else
                {
                    return Task.FromResult(new { Message = "Timeout after 20 seconds" });
                }
            };

            Post["/enroll/image", true] = async (parameters, ct) =>
            {
                var verifyReqPayload = this.Bind<VerificationRequestMediaType>();

                JsonSettings.MaxJsonLength = 50 * 10000;
                var verifyTask = service.VerifyAsync(verifyReqPayload.CaptureId, verifyReqPayload.EnrolledIds);

                if (verifyTask == await Task.WhenAny(verifyTask, Task.Delay(TIMEOUT_SECONDS * 1000)))
                {
                    return await verifyTask;
                }
                else
                {
                    return Task.FromResult(new { Message = "Timeout after 20 seconds" });
                }
            };

        }
    }
}

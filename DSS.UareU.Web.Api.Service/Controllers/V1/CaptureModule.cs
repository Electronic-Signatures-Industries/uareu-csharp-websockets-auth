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

        }
    }
}

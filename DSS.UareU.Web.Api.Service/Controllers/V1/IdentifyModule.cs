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
    public class IdentifyModule : NancyModule
    {
        const int TIMEOUT_SECONDS = 10;
        DPUareUReaderService service = new DPUareUReaderService();

        public IdentifyModule() : base("/api/v1")
        {
            Post["/verify", true] = async (parameters, ct) =>
            {                
                JsonSettings.MaxJsonLength = 50 * 10000;
                var verifyTask = service.VerifyAsync();

                if (verifyTask == await Task.WhenAny(verifyTask, Task.Delay(TIMEOUT_SECONDS * 1000))) {
                    return await verifyTask;
                } else {
                    return Task.FromResult(new { Message = "Timeout after 10 seconds" });
                }
            };

        }
        
    }
}

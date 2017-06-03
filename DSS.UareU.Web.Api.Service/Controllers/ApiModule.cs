using DSS.A2F.Fingerprint.License.Shared;
using Jose;
using Nancy;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSS.UareU.Web.Api.Service.Controllers
{
    public class ApiModule : NancyModule
    {
        public ApiModule() : base("/api")
        {
            //Get["/demo_service_token"] = parameters =>
            //{

            //    var secretKey = ((LicenseModel)this.Context.Items["License"]).ApiServerSecret;
            //    var payload = new Dictionary<string, object>()
            //    {
            //        { "email", "molekilla@gmail.com" },
            //        { "exp", DateTime.UtcNow.AddHours(12).ToBinary() }
            //    };


            //    return new { Token = Jose.JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS256) };
            //};

            Get["/health"] = parameters =>
            {
                return "OK";
            };

        }
    }
}

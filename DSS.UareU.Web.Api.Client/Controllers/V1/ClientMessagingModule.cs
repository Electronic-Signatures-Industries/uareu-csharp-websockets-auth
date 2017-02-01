using DSS.UareU.Web.Api.Shared.Mediatypes;
using DSS.UareU.Web.Api.Client.Properties;
using Nancy;
using Nancy.Json;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DSS.UareU.Web.Api.Client.Controllers.V1
{
    //public class ClientMessagingModule : NancyModule
    //{
    //    public ClientMessagingModule() : base("/api/v1/client_messaging")
    //    {
    //        Get["/"] = (parameters) =>
    //        {
    //            var m = Encoding.UTF8.GetBytes(Resources.ClientMessagingWrapper);
    //            return new Nancy.Responses.HtmlResponse {
    //                StatusCode = HttpStatusCode.OK,
    //                Contents = a => a.Write(m, 0, m.Length),
    //            };
    //        };


    //    }

    //}
}

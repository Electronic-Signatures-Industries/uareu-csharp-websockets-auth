using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Services
{
    class SampleService
    {

        //public Nancy.Response RequestWebParse(Nancy.Request request, string serviceName, ParseMediaType payload)
        //{
        //    try
        //    {
        //        var body = RequestWebParse(serviceName, payload);

        //        // 202
        //        Nancy.Response response = new Response();
        //        response.StatusCode = HttpStatusCode.Accepted;
        //        response.Headers["Location"] = body.Headers.FirstOrDefault(t => t.Name == "Location").Value.ToString(); ;

        //        return response;
        //    }
        //    catch (Exception e)
        //    {
        //        return new Response { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = e.Message };
        //    }
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Shared
{
    public static class ResponseMessageBuilder
    {
        public static Nancy.Response BuildLocationResponse(Nancy.HttpStatusCode code, string uri)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("Location", uri);
            return new Nancy.Response
            {
                StatusCode = code,
                Headers = dict
            };
        }

        public static Nancy.Response BuildMessageResponse(Nancy.HttpStatusCode code, string message)
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

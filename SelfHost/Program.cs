using DSS.UareU.Web.Api.Service.Controllers.V1;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace ServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            //string wsPort = ConfigurationManager.AppSettings["auth2factor.Websocket.Port"];
            var wssv = new WebSocketServer(Int32.Parse("8082"));
            //wssv.SslConfiguration.ServerCertificate = new X509Certificate2(Resources.certificate, "a2f");
            wssv.AddWebSocketService<ReaderClientController>("/reader");
            wssv.Start();

            var url = "http://localhost:8080";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }
}
using DSS.UareU.Web.Api.Service.Controllers.V1;
using Microsoft.Owin.Hosting;
using ServiceHost;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace SelfHost
{
    public class ServiceSelfHost
    {
        IDisposable service;
        WebSocketServer wss;

        public void Start()
        {
            string restHost = ConfigurationManager.AppSettings["RestHostUrl"];
            string wsPort = ConfigurationManager.AppSettings["WebsocketPort"];

            this.wss = new WebSocketServer(Int32.Parse(wsPort));
            //wssv.SslConfiguration.ServerCertificate = new X509Certificate2(Resources.certificate, "a2f");
            wss.AddWebSocketService<ReaderClientController>("/reader");
            wss.Start();

            service = WebApp.Start<Startup>(restHost);
        }

        public void Stop()
        {
            service.Dispose();
            wss.Stop();
            Console.WriteLine("Stopped. Good bye!");
        }
    }
}


using DSS.UareU.Web.Api.Service.Controllers.V1;
using Microsoft.Owin.Hosting;
using SelfHost;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using WebSocketSharp.Server;

namespace ServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<ServiceSelfHost>(s =>
                {
                    s.ConstructUsing(name => new ServiceSelfHost());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();
                x.SetDescription("auth2factor fingerprint service");
                x.SetDisplayName("auth2factor fingerprint service");
                x.SetServiceName("auth2factor fingerprint");
            });
            //try
            //{
            //    string restHost = ConfigurationManager.AppSettings["RestHostUrl"];
            //    string wsPort = ConfigurationManager.AppSettings["WebsocketPort"];
            //    var wssv = new WebSocketServer(Int32.Parse(wsPort));
            //    //wssv.SslConfiguration.ServerCertificate = new X509Certificate2(Resources.certificate, "a2f");
            //    wssv.AddWebSocketService<ReaderClientController>("/reader");
            //    wssv.Start();

            //    using (WebApp.Start<Startup>(restHost))
            //    {
            //        Console.WriteLine("Running REST on {0}, ws on {1}", restHost, wsPort);
            //        Console.WriteLine("Press enter to exit");
            //        Console.ReadLine();
            //    }
            //}
            //catch (Exception e)
            //{

            //    Console.WriteLine("Failed to load, please review configuration settings.");
            //    Console.Write(e.Message);
            //}
        }
    }
}
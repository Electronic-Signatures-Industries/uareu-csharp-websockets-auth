﻿using ClientHost.Properties;
using DSS.UareU.Web.Api.Client.Controllers.V1;
using DSS.UareU.Web.Api.Client.Services;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace ClientHost
{
    class Program
    {
        static void Main(string[] args)
        {
            //string wsPort = ConfigurationManager.AppSettings["auth2factor.Websocket.Port"];
            //var wssv = new WebSocketServer(Int32.Parse(wsPort), true);
            //wssv.SslConfiguration.ServerCertificate = new X509Certificate2(Resources.certificate, "a2f");
            //wssv.AddWebSocketService<PingWebSocketController>("/ping");
            //wssv.Start();

            ReaderWebSocketClientService client = new ReaderWebSocketClientService();
            client.Start("cliente");

            string webPort = ConfigurationManager.AppSettings["auth2factor.REST.Port"];
            var url = "http://localhost:" + webPort;

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
                client.Close();
            }
        }
    }
}
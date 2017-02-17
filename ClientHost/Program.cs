using ClientHost.Properties;
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

            string url = "ws://a2factorfingerprintdemo.cloudapp.net:8082/reader";
            ReaderWebSocketClientService client = new ReaderWebSocketClientService(url);
            client.Start("cliente");

                Console.ReadLine();
                client.Close("cliente");
        }
    }
}
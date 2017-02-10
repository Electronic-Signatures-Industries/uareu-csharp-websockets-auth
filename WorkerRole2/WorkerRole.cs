using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using WebSocketSharp.Server;
using Microsoft.Owin.Hosting;
using DSS.UareU.Web.Api.Service.Controllers.V1;

namespace DSS.A2F.Fingerprint.Owin.Role
{
    public class WorkerRole : RoleEntryPoint
    {
        private IDisposable _app = null;
        private WebSocketServer _wssv = null;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("DSS.A2F.Fingerprint.Owin.Role is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;

            var endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["REST"];
            string baseUri = String.Format("{0}://{1}",
                endpoint.Protocol, endpoint.IPEndpoint);

            var wsEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WS"];
            string wsUrl = String.Format("{0}://{1}", "ws", wsEndpoint.IPEndpoint);

            Trace.TraceInformation(String.Format("Starting OWIN at {0}", baseUri),
                "Information");
            _wssv = new WebSocketServer(wsUrl);

            //wssv.SslConfiguration.ServerCertificate = new X509Certificate2(Resources.certificate, "a2f");
            _wssv.AddWebSocketService<ReaderClientController>("/reader");
            _wssv.Start();
            _app = WebApp.Start<Startup>(new StartOptions(url: baseUri));
            return base.OnStart();
        }

        public override void OnStop()
        {
            if (_wssv != null)
            {
                _wssv.Stop();
            }
            if (_app != null)
            {
                _app.Dispose();
            }
            base.OnStop();
        }
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}

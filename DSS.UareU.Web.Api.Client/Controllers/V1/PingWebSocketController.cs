using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DSS.UareU.Web.Api.Client.Controllers.V1
{
    public class PingWebSocketController : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Send("OK");
        }
    }
}

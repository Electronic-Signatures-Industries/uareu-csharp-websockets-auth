using DSS.UareU.Web.Api.Shared.Mediatypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace DSS.UareU.Web.Api.Client.Services
{
    public class ReaderWebSocketClientService
    {
        private WebSocket client { get; set; }
        ReaderService reader = new ReaderService();

        public ReaderWebSocketClientService()
        {
            client = new WebSocket("ws://localhost:8082/reader");
            client.OnMessage += Client_OnMessage;
        }

        public void Start()
        {
            this.client.Connect();
            this.client.Send("REGISTER_DEVICE");
        }

        public void Close()
        {
            this.client.Close();
        }

        private async void Client_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                var payload = JsonConvert.DeserializeObject<ReaderClientRequestMediaType>(e.Data);
                var request = new ReaderClientRequestMediaType
                {
                    StateCheck = payload.StateCheck,
                };
                switch (payload.Type)
                {
                    case "device_info_request":
                        request.Type = "device_info_reply";
                        var info = await reader.GetReaderInfo();
                        request.Data = JsonConvert.SerializeObject(info);
                        this.client.Send(JsonConvert.SerializeObject(request));
                        break;

                }
            }
        }
    }
}

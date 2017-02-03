using DSS.UareU.Web.Api.Shared.Mediatypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DSS.UareU.Web.Api.Service.Controllers.V1
{
    public class ReaderClientController : WebSocketBehavior
    {
        static Dictionary<string, string> RequestSubscribers = new Dictionary<string, string>();
        static Dictionary<string, string> DevicesSubscribers = new Dictionary<string, string>();

        public ReaderClientController()
        {
        }

        void SendToDevices(ReaderClientRequestMediaType request)
        {
            foreach (var id in this.Sessions.ActiveIDs.Where(i => DevicesSubscribers.FirstOrDefault(j => j.Key == i).Value != null))
            {
                this.Sessions.SendTo(JsonConvert.SerializeObject(request), id);
            }
        }

        protected override void OnMessage(MessageEventArgs e)
        {

            if (e.IsText)
            {
                if (e.Data == "REGISTER_DEVICE")
                {
                    DevicesSubscribers.Add(this.ID, this.ID);
                    return;
                }
                var payload = JsonConvert.DeserializeObject<ReaderClientRequestMediaType>(e.Data);
                var request = new ReaderClientRequestMediaType
                {
                    StateCheck = payload.StateCheck,
                    Data = payload.Data,
                };

                var sid = string.Empty;
                if (RequestSubscribers.FirstOrDefault(i => i.Key == payload.StateCheck).Value != null)
                {
                    sid = RequestSubscribers[payload.StateCheck];
                }
                switch (payload.Type)
                {
                    case "device_info":
                        if (RequestSubscribers.FirstOrDefault(i => i.Key == payload.StateCheck).Value == null) {
                            RequestSubscribers.Add(payload.StateCheck, this.ID);
                        }
                        request.Type = payload.Type + "_request";

                        SendToDevices(request);
                        break;

                    case "device_info_reply":
                        if (sid != null)
                        {
                            RequestSubscribers.Remove(payload.StateCheck);
                            request.Type = "device_info_response";
                            this.Sessions.SendTo(JsonConvert.SerializeObject(request), sid);
                        }
                        break;

                    case "capture_image":
                        if (RequestSubscribers.FirstOrDefault(i => i.Key == payload.StateCheck).Value == null)
                        {
                            RequestSubscribers.Add(payload.StateCheck, this.ID);
                        }
                        request.Type = payload.Type + "_request";

                        SendToDevices(request);
                        break;

                    case "capture_image_reply":
                        if (sid != null)
                        {
                            RequestSubscribers.Remove(payload.StateCheck);
                            request.Type = "capture_image_response";
                            this.Sessions.SendTo(JsonConvert.SerializeObject(request), sid);
                        }
                        break;


                }
            }
        }
    }
}

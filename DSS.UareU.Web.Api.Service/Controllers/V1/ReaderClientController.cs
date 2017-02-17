using DSS.UareU.Web.Api.Service.Services;
using DSS.UareU.Web.Api.Shared.Mediatypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DSS.UareU.Web.Api.Service.Controllers.V1
{
    public class ReaderClientController : WebSocketBehavior
    {
        static Dictionary<string, bool> OnPreReponseQueue = new Dictionary<string, bool>();
        static Dictionary<string, string> RequestSubscribers = new Dictionary<string, string>();
        static MemoryCache DevicesSubscribers = new MemoryCache("device-subscribers");
        CacheItemPolicy CACHE_POLICY;

        WebSocketSecureTokenService AuthService { get; set; }
        CaptureService captureService = new CaptureService();

        public ReaderClientController()
        {
            AuthService = new WebSocketSecureTokenService();
            AuthService.BindLicense();

            CACHE_POLICY = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromHours(1),
            };
        }

        void SendToDevices(ReaderClientRequestMediaType request)
        {
            foreach (var id in this.Sessions.ActiveIDs.Where(i => DevicesSubscribers.FirstOrDefault(j => j.Key == i).Value != null))
            {
                this.Sessions.SendTo(JsonConvert.SerializeObject(request), id);
            }
        }

        void Reply(string stateCheck, ReaderClientRequestMediaType request)
        {
            var sid = string.Empty;

            if (RequestSubscribers.FirstOrDefault(i => i.Key == stateCheck).Value != null)
            {
                sid = RequestSubscribers[stateCheck];
            }

            if (sid.Length > 0)
            {
                RequestSubscribers.Remove(stateCheck);

                if (this.Sessions.ActiveIDs.Where(i => i == sid).Count() > 0)
                {
                    this.Sessions.SendTo(JsonConvert.SerializeObject(request), sid);
                }
            }
        }



        void SendToDevice(string clientId, ReaderClientRequestMediaType request)
        {
            if (DevicesSubscribers.FirstOrDefault(i => i.Key == clientId).Value == null)
            {
                request.Data = JsonConvert.SerializeObject(new { Message = "Invalid client id" });
                this.Send(JsonConvert.SerializeObject(request));
                return;
            }

            var id = (string)DevicesSubscribers[clientId];
            if (this.Sessions.ActiveIDs.Where(i => i == id).Count() > 0)
            {
                this.Sessions.SendTo(JsonConvert.SerializeObject(request), id);
            }
        }

        void RequiresAuthentication(string type, ReaderClientRequestMediaType request, out bool exit)
        {
            exit = false;
            if (!AuthService.IsAuthenticated)
            {
                request.Type = type;
                request.Data = JsonConvert.SerializeObject(new { Message = "Invalid token or expired" });
                this.Send(JsonConvert.SerializeObject(request));
                exit = true;
            }
        }

        void RequiresOriginCheck(string type, ReaderClientRequestMediaType request, out bool exit)
        {
            exit = false;
            if (!AuthService.IsValidOrigin(this.Context.Origin))
            {
                request.Type = type;
                request.Data = JsonConvert.SerializeObject(new { Message = "Invalid origin, check license allowed app origins" });
                this.Send(JsonConvert.SerializeObject(request));
                exit = true;
            }
        }


        void DeviceConnectionMiddleware(string data, out bool next)
        {
            next = true;

            if (data.IndexOf("REGISTER_DEVICE") > -1)
            {
                var id = data.Split(':').ElementAtOrDefault(1);

                if (id == null)
                {
                    throw new Exception("Missing register device name");
                }

                if (DevicesSubscribers.FirstOrDefault(i => i.Key == id).Value == null)
                {
                    DevicesSubscribers.Add(id, this.ID, CACHE_POLICY);
                }
                next = false;
            }

            if (data.IndexOf("UNREGISTER_DEVICE") > -1)
            {
                var id = data.Split(':').ElementAtOrDefault(1);

                if (id == null)
                {
                    throw new Exception("Missing register device name");
                }

                if (DevicesSubscribers.FirstOrDefault(i => i.Key == id).Value != null)
                {
                    DevicesSubscribers.Remove(id);
                }
                next = false;
            }
        }

        protected override void OnMessage(MessageEventArgs e)
        {

            if (e.IsText)
            {
                var next = true;
                DeviceConnectionMiddleware(e.Data, out next);
                if (!next)
                {
                    return;
                }

                var payload = JsonConvert.DeserializeObject<ReaderClientRequestMediaType>(e.Data);
                AuthService.BindToken(payload.BearerToken);

                var request = new ReaderClientRequestMediaType
                {
                    StateCheck = payload.StateCheck,
                    Data = payload.Data,
                };
                bool exit = false;

                switch (payload.Type)
                {
                    case "device_subscribers":
                        this.RequiresOriginCheck(payload.Type, request, out exit);
                        if (exit) return;

                        this.RequiresAuthentication(payload.Type, request, out exit);
                        if (exit) return;

                        request.Type = payload.Type + "_response";
                        request.Data = JsonConvert.SerializeObject(new { devices = DevicesSubscribers.ToArray() });
                        SendToDevice(this.ID, request);
                        break;
                    case "device_info":
                        this.RequiresOriginCheck(payload.Type, request, out exit);
                        if (exit) return;

                        this.RequiresAuthentication(payload.Type, request, out exit);
                        if (exit) return;

                        if (RequestSubscribers.FirstOrDefault(i => i.Key == payload.StateCheck).Value == null)
                        {
                            RequestSubscribers.Add(payload.StateCheck, this.ID);
                        }
                        request.Type = payload.Type + "_request";

                        SendToDevice(payload.ClientID, request);
                        break;

                    case "device_info_reply":
                        request.Type = "device_info_response";
                        Reply(payload.StateCheck, request);
                        break;

                    case "capture_image":
                        this.RequiresOriginCheck(payload.Type, request, out exit);
                        if (exit) return;

                        this.RequiresAuthentication(payload.Type, request, out exit);
                        if (exit) return;

                        if (RequestSubscribers.FirstOrDefault(i => i.Key == payload.StateCheck).Value == null)
                        {
                            RequestSubscribers.Add(payload.StateCheck, this.ID);
                        }
                        request.Type = payload.Type + "_request";

                        if (payload.Data != null && payload.Data.Length > 0)
                        {
                            try
                            {
                                JObject options = JsonConvert.DeserializeObject(payload.Data) as JObject;
                                bool store = options["store"].Value<bool>();
                                if (store)
                                {
                                    OnPreReponseQueue.Add(payload.StateCheck, true);
                                }
                            }
                            catch
                            {
                                // ignore
                            }
                        }
                        SendToDevice(payload.ClientID, request);
                        break;

                    case "capture_image_reply":
                        request.Type = "capture_image_response";

                        if (OnPreReponseQueue.Keys.FirstOrDefault(i => i == payload.StateCheck) != null)
                        {
                            OnPreReponseQueue.Remove(payload.StateCheck);
                            var capture = JsonConvert.DeserializeObject<FingerCaptureClient>(request.Data);
                            var model = captureService.SaveCapture(capture.FMD, capture.WSQ);
                            capture.ID = model.Id;

                            request.Data = JsonConvert.SerializeObject(capture);

                        }
                        Reply(payload.StateCheck, request);
                        break;


                }
            }
        }
    }
}

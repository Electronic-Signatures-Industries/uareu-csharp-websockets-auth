using DSS.A2F.Fingerprint.Api.Shared.Mediatypes;
using DSS.UareU.Web.Api.Service.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DSS.UareU.Web.Api.Service.Controllers.V1
{
    public class ReaderClientController : WebSocketBehavior
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        static MemoryCache OnPreReponseQueue = new MemoryCache("on-pre-response");
        static MemoryCache RequestSubscribers = new MemoryCache("request-subscribers");
        static MemoryCache DevicesSubscribers = new MemoryCache("device-subscribers");
        CacheItemPolicy CACHE_POLICY;
        CacheItemPolicy REQ_CACHE_POLICY;

        WebSocketSecureTokenService AuthService { get; set; }
        CaptureService captureService = new CaptureService();

        public ReaderClientController()
        {
            AuthService = new WebSocketSecureTokenService();
            logger.Info("bind license");
            AuthService.BindLicense();

            CACHE_POLICY = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromHours(1),
            };

            REQ_CACHE_POLICY = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(15),
            };
        }

        void SendToDevices(ReaderClientRequestMediaType request)
        {
            logger.Info("send to devices");
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
                sid = (string)RequestSubscribers[stateCheck];
            }

            if (sid.Length > 0)
            {
                RequestSubscribers.Remove(stateCheck);

                if (this.Sessions.ActiveIDs.Where(i => i == sid).Count() > 0)
                {
                    logger.Info("reply to subscriber if active: sid={0}, state={1}", sid, stateCheck);
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
                logger.Info("send to  device if active: sid={0}", id);
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

        void AddSessionToRequestSubscriber(string stateCheck)
        {
            if (RequestSubscribers.FirstOrDefault(i => i.Key == stateCheck).Value == null)
            {
                RequestSubscribers.Add(stateCheck, this.ID, this.REQ_CACHE_POLICY);
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
                    logger.Info("device '{0}' registered", id);
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
                    logger.Info("device '{0}' unregistered", id);
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

                        logger.Info("listing device subscribers");
                        request.Type = payload.Type + "_response";
                        request.Data = JsonConvert.SerializeObject(new { devices = DevicesSubscribers.ToArray() });
                        SendToDevice(this.ID, request);
                        break;
                    case "device_info":
                        this.RequiresOriginCheck(payload.Type, request, out exit);
                        if (exit) return;

                        this.RequiresAuthentication(payload.Type, request, out exit);
                        if (exit) return;

                        logger.Info("requesting device info");
                        AddSessionToRequestSubscriber(payload.StateCheck);
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

                        logger.Info("requesting capture image");
                        AddSessionToRequestSubscriber(payload.StateCheck);
                        request.Type = payload.Type + "_request";

                        if (payload.Data != null && payload.Data.Length > 0)
                        {
                            try
                            {
                                JObject options = JsonConvert.DeserializeObject(payload.Data) as JObject;
                                bool store = options["store"].Value<bool>();
                                if (store)
                                {
                                    OnPreReponseQueue.Add(payload.StateCheck, true, REQ_CACHE_POLICY);
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

                        try
                        {
                            if (OnPreReponseQueue.FirstOrDefault(i => i.Key == payload.StateCheck).Value != null)
                            {
                                OnPreReponseQueue.Remove(payload.StateCheck);
                                var capture = JsonConvert.DeserializeObject<FingerCaptureClient>(request.Data);
                                var model = captureService.SaveCapture(capture.FMD, capture.WSQ);
                                capture.ID = model.Id;

                                request.Data = JsonConvert.SerializeObject(capture);
                            }
                        }
                        catch (Exception ex)
                        {
                            request.Data = JsonConvert.SerializeObject(new
                            {
                                error = ex.ToString(),
                            });
                        }

                        Reply(payload.StateCheck, request);
                        break;
                }
            }
        }
    }
}

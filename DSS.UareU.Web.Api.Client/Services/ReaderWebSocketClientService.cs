using DSS.A2F.Fingerprint.Api.Shared.Mediatypes;
using Newtonsoft.Json;
using NLog;
using System.Threading.Tasks;
using WebSocketSharp;

namespace DSS.UareU.Web.Api.Client.Services
{
    public class ReaderWebSocketClientService
    {
        private WebSocket client { get; set; }
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        ReaderService reader = new ReaderService();
        const int TIMEOUT_SECONDS = 15;

        public ReaderWebSocketClientService(string url)
        {
            logger.Info("opening {0}", url);
            client = new WebSocket(url);
            client.OnMessage += Client_OnMessage;
        }

        public void Start(string name)
        {
            this.client.Connect();
            this.client.Send("REGISTER_DEVICE:" + name);
            logger.Info("connecting");
        }

        public void Close(string name)
        {
            this.client.Send("UNREGISTER_DEVICE:" + name);
            this.client.Close();
            logger.Info("closing");
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
                        logger.Info("requesting device info");
                        request.Type = "device_info_reply";

                        try
                        {
                            var info = await reader.GetReaderInfo();
                            request.Data = JsonConvert.SerializeObject(info);
                        }
                        catch
                        {
                            logger.Info("no device found");
                            request.Data = JsonConvert.SerializeObject(new { Message = "No device found" });
                        }

                        if (this.client.IsAlive)
                        {
                            this.client.Send(JsonConvert.SerializeObject(request));
                        }

                        break;

                    case "capture_image_request":
                        request.Type = "capture_image_reply";
                        var captureTask = reader.CaptureAsync();
                        logger.Info("requesting capture");
                        if (captureTask == await Task.WhenAny(captureTask, Task.Delay(TIMEOUT_SECONDS * 1000)))
                        {
                            await captureTask;
                            await reader.GetCaptureImageAsync(this.reader.CurrentCaptureID, new FindCaptureOptions
                            {
                                Extended = false,
                            });

                            if (this.reader.CurrentCaptureModel == null)
                            {
                                logger.Info("no reader found");
                                reader.Close();
                                request.Data = JsonConvert.SerializeObject(new { Message = "No reader found" });
                            }
                            else
                            {
                                request.Data = JsonConvert.SerializeObject(new FingerCaptureClient
                                {
                                    Image = this.reader.CurrentCaptureModel.Image,
                                    FMD = this.reader.CurrentCaptureModel.FMD,
                                    WSQ = this.reader.CurrentCaptureModel.WSQImage,
                                    ContentType = "image/jpg",
                                });
                                logger.Info("capture done");
                            }
                        }
                        else
                        {
                            logger.Info("timeout");
                            reader.Close();
                            request.Data = JsonConvert.SerializeObject(new { Message = "Timeout after 15 seconds" });
                        }


                        if (this.client.IsAlive)
                        {
                            this.client.Send(JsonConvert.SerializeObject(request));
                        }

                        break;

                }
            }
        }
    }
}

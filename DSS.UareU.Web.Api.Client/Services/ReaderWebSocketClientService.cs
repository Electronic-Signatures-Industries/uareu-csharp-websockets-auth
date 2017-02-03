﻿using DSS.UareU.Web.Api.Shared.Mediatypes;
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
        const int TIMEOUT_SECONDS = 15;

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

                    case "capture_image_request":
                        request.Type = "device_info_reply";
                        var captureTask = reader.CaptureAsync("mol@killa");

                        if (captureTask == await Task.WhenAny(captureTask, Task.Delay(TIMEOUT_SECONDS * 1000)))
                        {
                            await captureTask;
                            await reader.GetCaptureImageAsync(this.reader.CurrentCaptureID, new FindCaptureOptions
                            {
                                Extended = false,
                            });
                            request.Data = JsonConvert.SerializeObject(new {
                                ID = reader.CurrentCaptureID,
                                Image = Convert.ToBase64String(this.reader.CurrentCaptureModel.Image),
                            });
                        }
                        else
                        {
                            reader.Close();
                            request.Data = JsonConvert.SerializeObject(new { Message = "Timeout after 15 seconds" });
                        }



                        this.client.Send(JsonConvert.SerializeObject(request));

                        break;

                }
            }
        }
    }
}

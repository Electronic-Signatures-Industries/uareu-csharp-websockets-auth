using DPUruNet;
using DSS.UareU.Web.Api.Client.Models;
using DSS.UareU.Web.Api.Shared;
using DSS.UareU.Web.Api.Shared.Mediatypes;
using Jose;
using Nancy.Responses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Client.Services
{
    public class ReaderService
    {
        CacheItemPolicy CACHE_POLICY = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTime.Now.AddHours(1)
        };
        Reader _reader;
        static MemoryCache _cache = new MemoryCache("dss-a2f-fp");
        public string CurrentCaptureID { get; set; }
        public FingerCapture CurrentCaptureModel { get; set; }

        public void Close()
        {
            if (this._reader != null)
            {
                this._reader.Dispose();
            }
        }

        private string GetSecureToken(string username, string id)
        {
            var secretKey = ConfigurationManager.AppSettings["TokenSecret"];
            var payload = new Dictionary<string, object>()
                {
                    { "account", id },
                    { "email", username },
                    { "exp", DateTime.UtcNow.AddHours(1).ToBinary() }
                };



            var s = Jose.JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS256);
            var token = Guid.NewGuid().ToString();
            ShortSecureTokens.Items.Add(token, s, this.CACHE_POLICY);

            return token;
        }

        private string SaveCapture(CaptureResult capture, Fid.Fiv imageView)
        {
            // FMD
            var fmd = CreateFMD(capture);

            // Image
            var img = BitmapConverter.CreateBitmap(imageView.RawImage, imageView.Width, imageView.Height);

            // Compress the image
            byte[] compressedData = DPUruNet.WSQ.CompressNIST(capture.Data, 94, 24000);

            MemoryStream stream = new MemoryStream();
            img.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;

            // var fmd2 = Importer.ImportFmd(fmd.Bytes, Constants.Formats.Fmd.ANSI, Constants.Formats.Fmd.ANSI);

            var model = new FingerCapture
            {
                FMD = fmd.Bytes,
                Image = stream.ToArray(),
                WSQImage = compressedData,
            };
            stream.Close();

            var id = Guid.NewGuid().ToString();
            _cache.Add(id, model, CACHE_POLICY);
            return id;

        }

        private Fmd CreateFMD(CaptureResult capture)
        {
            DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(capture.Data, Constants.Formats.Fmd.ANSI);
            if (resultConversion.ResultCode != Constants.ResultCode.DP_SUCCESS)
            {
                throw new Exception(resultConversion.ResultCode.ToString());
            }

            return resultConversion.Data;
        }

        public Task<Nancy.Response> UpdateCaptureModel(object a)
        {
            return null;
        }

        public Task GetCaptureImageAsync(string id, FindCaptureOptions options)
        {
            FingerCapture model = null;
            if (_cache[id] != null)
            {
                model = (FingerCapture)_cache[id];
            }

            if (model == null)
            {
                return Task.FromResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No capture found"));
            }

            this.CurrentCaptureModel = model;
            MemoryStream stream = new MemoryStream();

            if (options.Extended)
            {
                var fmd = Convert.ToBase64String(model.FMD);
                var wsq = Convert.ToBase64String(model.WSQImage);

                return Task.FromResult(new CaptureResponseMediaType { Fmd = fmd, Wsq = wsq });
            }
            else
            {
                stream.Write(model.Image, 0, model.Image.Length);
                stream.Position = 0;
                var resp = new StreamResponse(() => stream, Nancy.MimeTypes.GetMimeType(id + ".jpg"));
                return Task.FromResult<Nancy.Response>(resp);
            }

        }

        public Task<Nancy.Response> CaptureAsync(string username)
        {
            var readers = ReaderCollection.GetReaders();
            var tcs = new TaskCompletionSource<Nancy.Response>();

            if (readers.Count > 0)
            {
                if (_reader == null)
                {
                    _reader = readers.FirstOrDefault();
                }
                var opened = _reader.Open(Constants.CapturePriority.DP_PRIORITY_EXCLUSIVE);
                Thread.Sleep(550);
                Console.WriteLine("Opened: " + opened.ToString());

                if (opened == Constants.ResultCode.DP_SUCCESS)
                {
                    var flag = _reader.CaptureAsync(Constants.Formats.Fid.ANSI, 
                        Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT, 500);
                    Thread.Sleep(1500);
                    _reader.On_Captured += (res) =>
                    {
                        Console.WriteLine("Captured");
                        var view = res.Data.Views.FirstOrDefault();
                        if (view != null) {                            
                            var id = SaveCapture(res, view);
                            this.CurrentCaptureID = id;
                            // var token = GetSecureToken(username, id);
                            // send as Location, 201
                            var resp = ResponseMessageBuilder.BuildLocationResponse(Nancy.HttpStatusCode.Created, "api/v1/capture/" + id);
                            // send nancy resp
                            tcs.SetResult(resp);
                        } else
                        {
                            tcs.SetResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No image captured"));
                        }

                        _reader.CancelCapture();
                        Thread.Sleep(1500);
                        _reader.Dispose();
                    };
                }
                else
                {
                    tcs.SetResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, opened.ToString()));
                }
            } else
            {
                _reader.Dispose();
                tcs.SetResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No reader"));
            }

            return tcs.Task;
        }

        public Task<Reader.ReaderDescription> GetReaderInfo()
        {
            var readers = ReaderCollection.GetReaders();
            var tcs = new TaskCompletionSource<Reader.ReaderDescription>();

            if (readers.Count > 0)
            {
                var reader = readers.FirstOrDefault();
                if (reader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE) == Constants.ResultCode.DP_SUCCESS)
                {
                    tcs.SetResult(reader.Description);
                }
                else
                {
                    tcs.SetResult(null);
                }
                reader.Dispose();

            }
            else
            {
                tcs.SetCanceled();
            }

            return tcs.Task;

        }


    }
}

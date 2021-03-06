using DPUruNet;
using DSS.A2F.Fingerprint.Api.Shared;
using DSS.A2F.Fingerprint.Api.Shared.Mediatypes;
using DSS.UareU.Web.Api.Client.Models;
using Nancy.Responses;
using NLog;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Client.Services
{
    public class ReaderService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
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
                logger.Info("closing reader");
                this._reader.Dispose();
            }
        }


        private string SaveCapture(CaptureResult capture, Fid.Fiv imageView)
        {
            logger.Info("saving capture");
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

            //FileStream s = new FileStream(DateTime.Now.Ticks.ToString() + "-87132230-ri.wsq", FileMode.CreateNew);
            //s.Write(compressedData, 0, compressedData.Length);
            //s.Close();

            stream.Close();

            var id = Guid.NewGuid().ToString();
            _cache.Add(id, model, CACHE_POLICY);
            logger.Info("capture saved - {0}", id);
            return id;

        }

        private Fmd CreateFMD(CaptureResult capture)
        {
            DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(capture.Data, Constants.Formats.Fmd.ANSI);
            if (resultConversion.ResultCode != Constants.ResultCode.DP_SUCCESS)
            {
                logger.Error("unable to convert to FMD");
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
            logger.Info("get capture from cache - {0}", id);
            FingerCapture model = null;
            if (_cache.FirstOrDefault(i => i.Key == id).Value != null)
            {
                model = (FingerCapture)_cache[id];
            }

            if (model == null)
            {
                logger.Info("no capture found");
                return Task.FromResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No capture found"));
            }

            this.CurrentCaptureModel = model;
            MemoryStream stream = new MemoryStream();

            logger.Info("returning capture");
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

        public Task<Nancy.Response> CaptureAsync()
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
                logger.Info("reader opened with result: {0}", opened);
                if (opened == Constants.ResultCode.DP_SUCCESS)
                {
                    var flag = _reader.CaptureAsync(Constants.Formats.Fid.ANSI, 
                        Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT, 500);
                    Thread.Sleep(1500);
                    _reader.On_Captured += (res) =>
                    {
                        var view = res.Data.Views.FirstOrDefault();
                        if (view != null) {
                            Console.WriteLine("Captured");
                            logger.Info("captured image and saving");

                            var id = SaveCapture(res, view);
                            this.CurrentCaptureID = id;
                            // var token = GetSecureToken(username, id);
                            // send as Location, 201
                            var resp = ResponseMessageBuilder.BuildLocationResponse(Nancy.HttpStatusCode.Created, "api/v1/capture/" + id);
                            // send nancy resp
                            tcs.SetResult(resp);
                        } else
                        {
                            logger.Info("unable to capture image");

                            tcs.SetResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No image captured"));
                        }

                        _reader.CancelCapture();
                        Thread.Sleep(1500);
                        _reader.Dispose();
                    };
                }
                else
                {
                    logger.Warn("reader issues: {0}", opened);
                    tcs.SetResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, opened.ToString()));
                }
            } else
            {
                if (_reader != null)
                {
                    _reader.Dispose();
                }
                logger.Warn("no reader found");
                tcs.SetResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No reader found"));
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
                if (reader.Open(Constants.CapturePriority.DP_PRIORITY_EXCLUSIVE) == Constants.ResultCode.DP_SUCCESS)
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

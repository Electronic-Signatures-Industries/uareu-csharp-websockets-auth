using DPUruNet;
using DSS.UareU.Web.Api.Service.Models;
using MongoDB.Driver;
using Nancy.Responses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Services
{
    public class ReaderService : BaseService
    {
        Reader _reader;
        
        public void Close()
        {
            if (this._reader != null)
            {
                this._reader.Dispose();
            }
        }

        private string SaveCapture(CaptureResult capture, Fid.Fiv imageView)
        {

            var fmd = CreateFMD(capture);
            var img = CreateBitmap(imageView.RawImage, imageView.Width, imageView.Height);

            MemoryStream stream = new MemoryStream();
            img.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;

            // var fmd2 = Importer.ImportFmd(fmd.Bytes, Constants.Formats.Fmd.ANSI, Constants.Formats.Fmd.ANSI);

            var coll = FingerCapture.GetCollection();
            var model = new FingerCapture
            {
                FMD = fmd.Bytes,
                Image = stream.ToArray(),
            };
            coll.InsertOne(model);
            stream.Close();
            return model.Id;
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

        public Task<Nancy.Response> GetCaptureImageAsync(string id)
        {
            var filter = Builders<FingerCapture>.Filter.Where(i => i.Id == id);
            var coll = FingerCapture.GetCollection();
            var model = coll.Find(filter).FirstOrDefault();

            if (model == null)
            {
                return Task.FromResult(BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No capture found"));
            }

            MemoryStream stream = new MemoryStream(model.Image);
            // Image img = Image.FromStream(stream);
            // img.Save("test.jpg");
            stream.Position = 0;
            var resp = new StreamResponse(() => stream, Nancy.MimeTypes.GetMimeType(id + ".jpg"));

            return Task.FromResult<Nancy.Response>(resp);
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
                            // send as Location, 201
                            var resp = BuildLocationResponse(Nancy.HttpStatusCode.Created, "api/v1/capture/" + id);
                            // send nancy resp
                            tcs.SetResult(resp);
                        } else
                        {
                            tcs.SetResult(BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No image captured"));
                        }

                        _reader.CancelCapture();
                        Thread.Sleep(1500);
                        _reader.Dispose();
                    };
                }
                else
                {
                    tcs.SetResult(BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, opened.ToString()));
                }
            } else
            {
                tcs.SetResult(BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No reader"));
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

        public Bitmap CreateBitmap(byte[] bytes, int width, int height)
        {
            byte[] rgbBytes = new byte[bytes.Length * 3];

            for (int i = 0; i <= bytes.Length - 1; i++)
            {
                rgbBytes[(i * 3)] = bytes[i];
                rgbBytes[(i * 3) + 1] = bytes[i];
                rgbBytes[(i * 3) + 2] = bytes[i];
            }
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            for (int i = 0; i <= bmp.Height - 1; i++)
            {
                IntPtr p = new IntPtr(data.Scan0.ToInt64() + data.Stride * i);
                System.Runtime.InteropServices.Marshal.Copy(rgbBytes, i * bmp.Width * 3, p, bmp.Width * 3);
            }

            bmp.UnlockBits(data);

            return bmp;
        }

    }
}

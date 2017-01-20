using DPUruNet;
using DSS.UareU.Web.Api.Service.Extras;
using DSS.UareU.Web.Api.Service.Mediatypes;
using DSS.UareU.Web.Api.Service.Models;
using MongoDB.Driver;
using Nancy.Responses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Services
{
    public class ReaderService : BaseService
    {
        CacheItemPolicy CACHE_POLICY = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTime.Now.AddHours(2)
        };
        Reader _reader;
        static MemoryCache _cache = new MemoryCache("dss-a2f-fp");
        
        public void Close()
        {
            if (this._reader != null)
            {
                this._reader.Dispose();
            }
        }

        private string SaveCapture(bool useCache, CaptureResult capture, Fid.Fiv imageView)
        {
            // FMD
            var fmd = CreateFMD(capture);

            // Image
            var img = CreateBitmap(imageView.RawImage, imageView.Width, imageView.Height);

            // Compress the image
            byte[] compressedData = DPUruNet.WSQ.CompressNIST(capture.Data, 94, 24000);

            // Decompress the image
            // byte[] uncompressedData = DPUruNet.WSQ.UnCompressNIST(compressedData, WSQ.IMAGE_FORMAT.DPFJ_FID_ISO_19794_4_2005);

            MemoryStream stream = new MemoryStream();
            img.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;

            // var fmd2 = Importer.ImportFmd(fmd.Bytes, Constants.Formats.Fmd.ANSI, Constants.Formats.Fmd.ANSI);

            var model = new FingerCapture
            {
                FMD = Encryption.Encrypt(fmd.Bytes),
                Image = Encryption.Encrypt(stream.ToArray()),
                WSQImage = compressedData,
            };
            stream.Close();

            if (useCache)
            {
                var id = Guid.NewGuid().ToString();
                _cache.Add(id, model, CACHE_POLICY);
                return id;
            }
            else
            {
                var coll = FingerCapture.GetCollection();
                coll.InsertOne(model);
                return model.Id;
            }
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

        public Task GetCaptureImageAsync(string id, bool sendWSQ)
        {
            FingerCapture model = null;
            if (_cache[id] == null)
            {
                var filter = Builders<FingerCapture>.Filter.Where(i => i.Id == id);
                var coll = FingerCapture.GetCollection();
                model = coll.Find(filter).FirstOrDefault();
            } else
            {
                model = (FingerCapture)_cache[id];
            }

            if (model == null)
            {
                return Task.FromResult(BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No capture found"));
            }

            // decrypt
            var image = Encryption.Decrypt(model.Image);

            MemoryStream stream = new MemoryStream();

            if (sendWSQ)
            {
                var content = Convert.ToBase64String(model.WSQImage);
       
                return Task.FromResult(new CaptureResponseMediaType { Data = content });
            }
            else
            {
                stream.Write(image, 0, image.Length);
                stream.Position = 0;
                var resp = new StreamResponse(() => stream, Nancy.MimeTypes.GetMimeType(id + ".jpg"));
                return Task.FromResult<Nancy.Response>(resp);
            }

        }

        public Task<Nancy.Response> CaptureAsync(bool useMemCache)
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
                            var id = SaveCapture(useMemCache, res, view);
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
                _reader.Dispose();
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

        private Bitmap CreateBitmap(byte[] bytes, int width, int height)
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

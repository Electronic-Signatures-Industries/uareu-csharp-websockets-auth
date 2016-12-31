using DPUruNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Services
{
    public class DPUareUReaderService
    {
        Reader _reader;
        
        public void Close()
        {
            if (this._reader != null)
            {
                this._reader.Dispose();
            }
        }

        public Task<dynamic> VerifyAsync() 
        {
            var readers = ReaderCollection.GetReaders();
            var tcs = new TaskCompletionSource<dynamic>();

            if (readers.Count > 0)
            {
                var reader = readers.FirstOrDefault();
                var opened = reader.Open(Constants.CapturePriority.DP_PRIORITY_EXCLUSIVE);
                Thread.Sleep(550);
                Console.WriteLine("Opened: " + opened.ToString());

                if (opened == Constants.ResultCode.DP_SUCCESS)
                {
                    Thread.Sleep(1500);
                    tcs.SetResult(new {});
                }
                else
                {
                    tcs.SetResult(new { Message = opened.ToString() });
                }
            
                reader.Dispose();

            } else
            {
                tcs.SetException(new Exception("No reader"));
            }
            
            return tcs.Task;
        }

        private Nancy.Response BuildLocationResponse(Nancy.HttpStatusCode code)
        {
            return new Nancy.Response{
                StatusCode = code };
        }

        public Task<dynamic> CaptureAsync()
        {
            var readers = ReaderCollection.GetReaders();
            var tcs = new TaskCompletionSource<dynamic>();

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
                            var id = Guid.NewGuid().ToString();
                            var img = CreateBitmap(view.RawImage, view.Width, view.Height);
                            img.Save(id + ".jpg");
                            // save as guid.jpg
                            // send as Location, 201
                            // send nancy resp
                        }
                        _reader.CancelCapture();
                        Thread.Sleep(1500);
                        _reader.Dispose();
                        tcs.SetResult(item);
                    };
                }
                else
                {
                    tcs.SetResult(new { Message = opened.ToString() });
                }
            } else
            {
                tcs.SetException(new Exception("No reader"));
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

using DPUruNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Services
{
    public class DPUareUReaderService
    {
        Reader _reader;
        public Task<dynamic> Capture()
        {
            var readers = ReaderCollection.GetReaders();
            var tcs = new TaskCompletionSource<dynamic>();
            System.Timers.Timer t = new System.Timers.Timer();

            if (readers.Count > 0)
            {
                if (_reader == null)
                {
                    _reader = readers.FirstOrDefault();
                }
                var opened = _reader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE);
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
                        var item = new {
                            Data = System.Convert.ToBase64String(res.Data.Views[0].RawImage),
                            res.Data.Views[0].Width,
                            res.Data.Views[0].Height,
                            res.Data.FingerCount,
                            res.Data.Format,
                            res.Data.ImageResolution,
                        };
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

            //t.Interval = 10000;
            //t.Elapsed += (sender, e) => {
            //    tcs.SetException(new Exception("10 s timeout"));
            //    t.Stop();
            //};
            //t.Start();
            return tcs.Task;
        }

        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Reader_On_Captured(CaptureResult result)
        {
            throw new NotImplementedException();
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

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

            if (readers.Count > 0)
            {
                if (_reader != null)
                {
                    _reader.CancelCapture();
                    _reader.Dispose();
                }
                _reader = null;
                _reader = readers.FirstOrDefault();
                //Console.WriteLine(_reader.Status.Status);
                var opened = _reader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE);
                Console.WriteLine(opened);
                //Thread.Sleep(1000);
                if (opened == Constants.ResultCode.DP_SUCCESS)
                {
                    //_reader.CancelCapture();
                    var flag = _reader.CaptureAsync(Constants.Formats.Fid.ANSI, Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT, 500);
                    _reader.On_Captured += (res) =>
                    {
                        Console.WriteLine(res.ResultCode);
                        var item = new {
                            Data = System.Convert.ToBase64String(res.Data.Bytes),
                            res.Data.FingerCount,
                            res.Data.Format,
                            res.Data.ImageResolution,
                        };
                        _reader.CancelCapture();
                        _reader.Dispose();
                        tcs.SetResult(item);
                    };
                }
                else
                {
                    tcs.SetResult(null);
                }
            //    reader.Dispose();
            } else
            {
                tcs.SetCanceled();
            }

            return tcs.Task;
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

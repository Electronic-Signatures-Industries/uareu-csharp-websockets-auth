using DPUruNet;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Controllers
{
    public class ApiModule : NancyModule
    {
        public ApiModule() : base("/api")
        {
            Get["/health"] = parameters =>
            {
                return "OK";
            };

            Get["/info", true] = async (parameters, ct) =>
            {
                var readers = ReaderCollection.GetReaders();
                var tcs = new TaskCompletionSource<Reader.ReaderDescription>();

                if (readers.Count > 0)
                {
                    var reader = readers.FirstOrDefault();
                        if (reader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE) == Constants.ResultCode.DP_SUCCESS)
                        {
                            tcs.SetResult(reader.Description);
                        } else
                        {
                        tcs.SetResult(null);
                        }
                        reader.Dispose();

                } else
                {
                    tcs.SetCanceled();
                }

                return await tcs.Task;

            };

        }
    }
}

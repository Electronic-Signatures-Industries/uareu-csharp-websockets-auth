using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Services
{
    public class VerificationService
    {

        public Task<dynamic> VerifyAsync(string captureId, IEnumerable<string> enrollImages)
        {
            // Comparison.Identify(captureFMD, enrolledFMDs, 1, 0.6)
            var tcs = new TaskCompletionSource<dynamic>();

            // tcs.SetResult(new { Message = opened.ToString() });

            return tcs.Task;
        }

    }
}

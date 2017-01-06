using DPUruNet;
using DSS.UareU.Web.Api.Service.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Services
{
    public class ComparisonService
    {
        private const int DPFJ_PROBABILITY_ONE = 0x7fffffff;

        public async Task<dynamic> IdentifyAsync(string captureId, IEnumerable<string> enrollIds)
        {
            // var tcs = new TaskCompletionSource<dynamic>();
            // See the SDK documentation for an explanation on threshold scores.
            int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;

            var coll = FingerCapture.GetCollection();

            var current = await coll.FindAsync(
                Builders<FingerCapture>.Filter.Where(i => i.Id == captureId)
            );

            var enrolled = await coll.Find(
                Builders<FingerCapture>.Filter.In(i => i.Id, enrollIds)
            )
            .ToListAsync();

            /*
            IdentifyResult identifyResult  = Comparison.Identify(current.First().FMD, 0, enrolled.First(), thresholdScore, 2);
            if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
            {
                // throw new Exception(identifyResult.ResultCode.ToString());
            }
            */
            // tcs.SetResult(new { Message = opened.ToString() });

            return new { };
        }

        private Fmd LoadFMD(byte[] raw)
        {
            var res = Importer.ImportFmd(raw, Constants.Formats.Fmd.ANSI, Constants.Formats.Fmd.ANSI);

            if (res.ResultCode != Constants.ResultCode.DP_SUCCESS)
            {
                Console.WriteLine(res.ResultCode.ToString());
                return null;
            }

            return res.Data;
        }
    }
}

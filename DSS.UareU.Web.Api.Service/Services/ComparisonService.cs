using DPUruNet;
using DSS.UareU.Web.Api.Service.Extras;
using DSS.UareU.Web.Api.Service.Mediatypes;
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

        public async Task<ComparisonResponseMediaType> IdentifyAsync(string captureId, IEnumerable<string> enrollIds)
        {
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

            var result = new ComparisonResponseMediaType();
            var currentRow = current.FirstOrDefault();

            if (currentRow != null && enrolled.Count == enrollIds.Count())
            {
                var currentFMD = LoadFMD(currentRow.FMD);
                var enrolledFMD = enrolled.Select(i => LoadFMD(i.FMD));
                
                IdentifyResult identifyResult  = Comparison.Identify(currentFMD, 0, enrolledFMD, thresholdScore, 2);
                if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    throw new Exception(identifyResult.ResultCode.ToString());
                }

                if (identifyResult.Indexes.Length > 0)
                {
                    int matches = 0;
                    int noMatches = 0;

                    Action<int> calc =
                    i =>
                    {
                        if (i > 0)
                        {
                            noMatches++;
                        }
                        else
                        {
                            matches++;
                        }
                    };

                    foreach (var pair in identifyResult.Indexes)
                    {
                        Console.WriteLine(pair[0] + " " + pair[1]);
                        // calc(pair[0]); // no hit
                        calc(pair[1]); // hit
                    }

                    result.IsMatch = matches > noMatches;
                } else
                {
                    result.IsMatch = false;
                }
            }

            return result;
        }

        private Fmd LoadFMD(byte[] raw)
        {
            // decrypt
            var data = Encryption.Decrypt(raw);
            var res = Importer.ImportFmd(data, Constants.Formats.Fmd.ANSI, Constants.Formats.Fmd.ANSI);

            if (res.ResultCode != Constants.ResultCode.DP_SUCCESS)
            {
                Console.WriteLine(res.ResultCode.ToString());
                return null;
            }

            return res.Data;
        }
    }
}

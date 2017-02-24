using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.A2F.Fingerprint.Api.Shared.Mediatypes
{
    public class ComparisonRequestMediaType
    {
        public string CaptureId { get; set; }
        public string[] EnrolledIds { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Mediatypes
{
    public class ComparisonRequestMediaType
    {
        public string CaptureId { get; set; }
        public string[] EnrolledIds { get; set; }
    }
}

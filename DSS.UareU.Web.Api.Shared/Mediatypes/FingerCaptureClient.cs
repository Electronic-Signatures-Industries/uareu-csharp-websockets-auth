using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.A2F.Fingerprint.Api.Shared.Mediatypes
{
    public class FingerCaptureClient
    {
        public string ID { get; set; }
        public byte[] Image { get; set; }
        public byte[] FMD { get; set; }
        public byte[] WSQ { get; set; }
        public string ContentType { get; set; }
    }
}

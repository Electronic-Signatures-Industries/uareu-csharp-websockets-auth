using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.A2F.Fingerprint.Api.Shared.Mediatypes
{
    public class ReaderClientRequestMediaType
    {
        public string BearerToken { get; set; }
        public string Type { get; set; }
        public string ClientID { get; set; }
        public string StateCheck { get; set; }
        public string Data { get; set; }
    }
}

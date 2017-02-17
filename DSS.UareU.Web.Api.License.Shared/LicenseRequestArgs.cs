using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.A2F.Fingerprint.License.Shared
{
    public class LicenseRequestArgs
    {
        public string Name { get; set; }
        public string Entity { get; set; }
        public List<string> AllowedApps { get; set; }
    }
}

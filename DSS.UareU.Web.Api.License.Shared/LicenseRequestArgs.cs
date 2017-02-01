using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.License.Shared
{
    public class LicenseRequestArgs
    {
        public string Name { get; set; }
        public string Entity { get; set; }
        public List<string> AllowedApps { get; set; }
    }
}

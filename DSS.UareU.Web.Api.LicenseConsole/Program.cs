using DSS.LicenseEngine;
using DSS.UareU.Web.Api.LicenseConsole.Properties;
using Fclp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.LicenseConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new FluentCommandLineParser<LicenseRequestArgs>();

            p.Setup(arg => arg.Name)
             .As('n', "name")
             .Required();

            p.Setup(arg => arg.Entity)
             .As('e', "entity")
             .Required();

            p.Setup<List<string>>(arg => arg.AllowedApps)
             .As('a', "allowedUrls")
             .Required();

            var result = p.Parse(args);

            if (result.HasErrors == false)
            {
                var lic = JsonConvert.SerializeObject(LicenseModel.FromArgs(p.Object));
                LicenseService signer = new LicenseService();
                var signature = signer.Sign(Resources.a2f_fp_key_pvk, lic);
            }
        }
    }
}

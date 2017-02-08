using DSS.LicenseEngine;
using DSS.UareU.Web.Api.LicenseConsole.Properties;
using Fclp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DSS.UareU.Web.Api.License.Shared;

namespace DSS.UareU.Web.Api.LicenseConsole
{
    // -n "Demo App" -e auth2factor -a http://localhost:8080/ http://localhost:8088/
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
                var model = LicenseModel.FromArgs(p.Object);
                var lic = JsonConvert.SerializeObject(model);
                LicenseService signer = new LicenseService();
                var signature = signer.Sign(Resources.a2f_fp_key_pvk, lic);
                model.l = signature;

                StreamWriter writer = new StreamWriter("license.json", false, Encoding.UTF8);
                lic = JsonConvert.SerializeObject(model);
                writer.Write(lic);
                writer.Close();

                Console.WriteLine("Created license.json");
                
            }
        }
    }
}

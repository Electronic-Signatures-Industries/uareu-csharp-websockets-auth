using SecurityDriven.Inferno;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.LicenseConsole
{
    public class LicenseModel
    {
        public string Name { get; set; }
        public string Entity { get; set; }
        public List<string> AllowedApps { get; set; }
        public string ApiServerKey { get; set; }
        public DateTime Created { get; set; }
        public string l { get; set; }

        public static LicenseModel FromArgs(LicenseRequestArgs a)
        {
            var model = new LicenseModel { Name = a.Name, Entity = a.Entity, AllowedApps = a.AllowedApps };
            model.Created = DateTime.Now;

            var rng = new CryptoRandom();
            using (SHA256 hash = SHA256Managed.Create())
            {
                byte[] result = hash.ComputeHash(rng.NextBytes(128));
                model.ApiServerKey = Convert.ToBase64String(result);
            }

            return model;
        }
    }
}

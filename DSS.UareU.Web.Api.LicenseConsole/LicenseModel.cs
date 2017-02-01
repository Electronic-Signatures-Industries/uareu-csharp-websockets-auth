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
        public string ApiServerSecret { get; set; }
        public string  ApiClientSecret { get; set; }
        public DateTime Created { get; set; }
        public string l { get; set; }

        public static LicenseModel FromArgs(LicenseRequestArgs a)
        {
            var model = new LicenseModel { Name = a.Name, Entity = a.Entity, AllowedApps = a.AllowedApps };
            model.Created = DateTime.Now;

            var rng = new CryptoRandom();
            using (SHA512 hash = SHA512Managed.Create())
            {
                byte[] result = hash.ComputeHash(rng.NextBytes(256));
                model.ApiServerSecret = Convert.ToBase64String(result);

                result = hash.ComputeHash(rng.NextBytes(256));
                model.ApiClientSecret = Convert.ToBase64String(result);
            }

            return model;
        }
    }
}

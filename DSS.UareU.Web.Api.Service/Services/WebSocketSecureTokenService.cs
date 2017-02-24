using DSS.A2F.Fingerprint.Api.Shared;
using DSS.A2F.Fingerprint.License.Shared;
using DSS.LicenseEngine;
using DSS.UareU.Web.Api.Service.Properties;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace DSS.UareU.Web.Api.Service.Services
{
    public class WebSocketSecureTokenService
    {
        public bool IsAuthenticated { get; set; }


        LicenseModel License { get; set; }

        public WebSocketSecureTokenService()
        {

        }

        public void BindLicense()
        {
            this.License = ApiBootstrap.GetLicense();

            try
            {

                var temp = new LicenseModel();
                var model = this.License;
                temp = new LicenseModel
                {
                    AllowedApps = model.AllowedApps,
                    ApiClientSecret = model.ApiClientSecret,
                    ApiServerSecret = model.ApiServerSecret,
                    Created = model.Created,
                    Entity = model.Entity,
                    Name = model.Name,
                };
                var unsignedLic = JsonConvert.SerializeObject(temp);
                LicenseService verifier = new LicenseService();
                bool isValid = verifier.Verify(Resources.a2f_fp_key_pub, this.License.l, unsignedLic);
                if (!isValid)
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while reading license.json");
                return;
            }
        }

        public void BindToken(string token)
        {
            try
            {
                var payloadJSON = Jose.JWT.Decode(token, Encoding.UTF8.GetBytes(this.License.ApiServerSecret));
                var payload = JsonConvert.DeserializeObject<JwtToken>(payloadJSON);
                var tokenExpires = DateTime.FromBinary(payload.exp);

                if (tokenExpires > DateTime.UtcNow)
                {
                    this.IsAuthenticated = true;
                }
                else
                {
                    this.IsAuthenticated = false;
                }


            }
            catch (Exception e)
            {
                this.IsAuthenticated = false;
            }
        }


        public bool IsValidOrigin(string origin)
        {
            return this.License.AllowedApps.Where(i => i.IndexOf(origin) > -1).Count() > 0;
        }
    }
}

using DSS.UareU.Web.Api.License.Shared;
using DSS.UareU.Web.Api.Service.Models;
using DSS.UareU.Web.Api.Shared;
using MongoDB.Driver;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service
{
    public class ApiBootstrap : IApplicationStartup
    {
        LicenseModel License { get; set;  }


        Nancy.Security.IUserIdentity OnBearerAuthentication(Nancy.NancyContext ctx)
        {
            ctx.Items.Add("License", this.License);
            var jwtToken = ctx.Request.Headers.Authorization.Replace("Bearer ", "");

            try
            {
                var payloadJSON = Jose.JWT.Decode(jwtToken, Encoding.UTF8.GetBytes(this.License.ApiServerSecret));
                var payload = JsonConvert.DeserializeObject<JwtToken>(payloadJSON);
                var tokenExpires = DateTime.FromBinary(payload.exp);

                if (tokenExpires > DateTime.UtcNow)
                {
                    ApiUser user = new ApiUser { UserName = payload.email };
                    var list = new List<string>();
                    list.Add(payload.email);
                    user.Claims = list;
                    return user;
                }

                return null;


            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void Initialize(IPipelines pipelines)
        {
            var license = ConfigurationManager.AppSettings["LicensePath"];
            license = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, license);
            if (!File.Exists(license))
            {
                throw new Exception("Missing license.json");
            }

            try
            {
                using (StreamReader reader = new StreamReader(license))
                {
                    var text = reader.ReadToEnd();
                    var model = JsonConvert.DeserializeObject<LicenseModel>(text);
                    this.License = model;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error while reading license.json");
            }

            var cs = ConfigurationManager.AppSettings["DbConnectionString"];
            var dbname = ConfigurationManager.AppSettings["DbName"];
            RegisterModels.Bind();
            DBClient.Instance = new MongoClient(cs);
            DBClient.Database = DBClient.Instance.GetDatabase(dbname);

            pipelines.AfterRequest += ctx =>
            {
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization");
                ctx.Response.Headers.Add("Access-Control-Expose-Headers", "Location");
                ctx.Response.Headers.Add("Access-Control-Allow-Methods", "DELETE, GET, POST, PUT");
            };

            var statelessAuthConfiguration = new StatelessAuthenticationConfiguration(OnBearerAuthentication);

            StatelessAuthentication.Enable(pipelines, statelessAuthConfiguration);
        }

    }
}

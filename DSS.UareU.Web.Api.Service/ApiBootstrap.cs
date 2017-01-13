using DSS.UareU.Web.Api.Service.Models;
using MongoDB.Driver;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service
{
    public class ApiBootstrap : IApplicationStartup
    {
        public void Initialize(IPipelines pipelines)
        {
            var cs = ConfigurationManager.AppSettings["DbConnectionString"];
            var dbname = ConfigurationManager.AppSettings["DbName"];
            RegisterModels.Bind();
            DBClient.Instance = new MongoClient(cs);
            DBClient.Database = DBClient.Instance.GetDatabase(dbname);

            pipelines.AfterRequest += ctx =>
            {
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization");
                ctx.Response.Headers.Add("Access-Control-Allow-Methods", "DELETE, GET, POST, PUT");
            };

            var secret = ConfigurationManager.AppSettings["TokenSecret"];
            var statelessAuthConfiguration = new StatelessAuthenticationConfiguration(ctx =>
            {
                var jwtToken = ctx.Request.Headers.Authorization.Replace("Bearer ", "");

                try
                {
                    var payloadJSON = Jose.JWT.Decode(jwtToken, Encoding.UTF8.GetBytes(secret));
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
            });

            StatelessAuthentication.Enable(pipelines, statelessAuthConfiguration);
        }

    }
}

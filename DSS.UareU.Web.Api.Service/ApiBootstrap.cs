using DSS.UareU.Web.Api.Service.Models;
using MongoDB.Driver;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
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

            var secret = ConfigurationManager.AppSettings["TokenSecret"];
            var statelessAuthConfiguration = new StatelessAuthenticationConfiguration(ctx =>
            {
                var jwtToken = ctx.Request.Headers.Authorization.Replace("Bearer ", "");

                try
                {
                    var payloadJSON = Jose.JWT.Decode(jwtToken, Encoding.UTF8.GetBytes(secret));
                    var payload = Jose.JWT.Decode<JwtToken>(jwtToken, Encoding.UTF8.GetBytes(secret));

                    var tokenExpires = DateTime.FromBinary(payload.exp);

                    if (tokenExpires > DateTime.UtcNow)
                    {
                        ApiUser user = new ApiUser { UserName = payload.sub };
                        var list = new List<string>();
                        list.Add(payload.account);
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

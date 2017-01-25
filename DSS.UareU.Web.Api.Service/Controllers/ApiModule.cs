using DPUruNet;
using DSS.UareU.Web.Api.Service.Models;
using DSS.UareU.Web.Api.Service.Services;
using Jose;
using MongoDB.Driver;
using Nancy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Controllers
{
    public class ApiModule : NancyModule
    {
        public ApiModule() : base("/api")
        {
            Get["/demo_token/{account}"] = parameters =>
           {
               var model = new Account
               {
                   Created = DateTime.Now,
                   Email = "molekilla@gmail.com",
                   Name = "Demo",
               };
               var accounts = Account.GetCollection();
               string id = parameters.account;
               var current = accounts.FindSync(
                   Builders<Account>.Filter.Where(i => i.Id == id)
               );

               var acct = current.FirstOrDefault();
               if (acct == null)
               {
                   accounts.InsertOne(model);
               } else
               {

                   model = acct;
               }

               var secretKey = ConfigurationManager.AppSettings["TokenSecret"];
               var payload = new Dictionary<string, object>()
                {
                    { "account", model.Id },
                    { "email", model.Email },
                    { "exp", DateTime.Now.AddHours(12).ToBinary() }
                };


               return new { Token = Jose.JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS256) };
           };

            Get["/health"] = parameters =>
            {
                return "OK";
            };

            Get["/v1/device_info", true] = async (parameters, ct) =>
            {
                ReaderService service = new ReaderService();
                return await service.GetReaderInfo();
            };

        }
    }
}

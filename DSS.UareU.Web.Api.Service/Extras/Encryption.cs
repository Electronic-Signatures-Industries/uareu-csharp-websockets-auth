﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Extras
{
    public class Encryption
    {
        public static byte[] Encrypt(byte[] data)
        {
            var key = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["EncKey"]);
            
            return SecurityDriven.Inferno.EtM_CBC.Encrypt(key, new ArraySegment<byte>(data));
        }

        public static byte[] Decrypt(byte[] cipher)
        {
            var key = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["EncKey"]);

            return SecurityDriven.Inferno.EtM_CBC.Decrypt(key, new ArraySegment<byte>(cipher));

        }
    }
}

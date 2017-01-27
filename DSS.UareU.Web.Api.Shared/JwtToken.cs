using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Shared
{
    public class JwtToken
    {
        public JwtToken()
        {

        }


        public Dictionary<string, string> sub { get; set; }
        public long exp { get; set; }
        public string email { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Models
{
    public class JwtToken
    {
        public string sub;
        public long exp;
        public string account;
    }
}

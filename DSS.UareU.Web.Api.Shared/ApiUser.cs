using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Shared
{
    public class ApiUser : Nancy.Security.IUserIdentity
    {
        public IEnumerable<string> Claims
        {
            get; set;
        }

        public string UserName
        {
            get; set;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace DSS.A2F.Fingerprint.Api.Shared
{
    public class ShortSecureTokens
    {
        public static MemoryCache Items = new MemoryCache("secure-tokens");

    }
}

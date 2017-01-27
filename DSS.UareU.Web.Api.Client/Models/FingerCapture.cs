using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Client.Models
{
    public class FingerCapture
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public byte[] Image { get; set; }
        public byte[] WSQImage { get; set; }
        public byte[] FMD { get; set; }

    }
}

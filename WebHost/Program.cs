using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebHost
{
    class Program
    {
        static void Main(string[] args)
        {
            HostConfiguration config = new HostConfiguration();
            config.UrlReservations.CreateAutomatically = true;
            using (var host = new NancyHost(config, new Uri("http://localhost:1234")))
            {
                host.Start();
                Console.WriteLine("Running on http://localhost:1234");
                Console.ReadLine();
            }
        }
    }
}

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MapTileService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://localhost:5000") // Allow remote calls http://*:5000 (instead of default http://localhost:5000)
                .UseStartup<Startup>()
                .Build();
    }
}

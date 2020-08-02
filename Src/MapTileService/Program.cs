using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MapTileService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            // TODO: single global config
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            var listenUrl = configuration.GetSection("listenUrl").Value;

            return WebHost.CreateDefaultBuilder(args)
                   .UseUrls(listenUrl)
                   .UseStartup<Startup>()
                   .Build();
        }
    }
}
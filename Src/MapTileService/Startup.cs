using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

using MapTileService.TileSources;

namespace MapTileService
{
    class Startup
    {
        public IConfiguration Configuration { get; set; }

        public static Dictionary<string, ITileSource> TileSources;

        public Startup()
        {
            var builder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json");
            this.Configuration = builder.Build();
            Startup.TileSources = Utils.GetTileSetConfigurations(this.Configuration)
                .ToDictionary(c => c.Name, c => TileSourceFabric.CreateTileSource(c));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton(this.Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.DisableTelemetry = true;
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}

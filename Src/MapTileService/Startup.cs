using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace MapTileService
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup()
        {
            var builder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton(Configuration);
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

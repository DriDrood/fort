using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Services;
using Fort.Utils;
using Fort.Utils.Logger;
using Fort.Utils.WS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fort
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
            configuration.GetSection("Fort").Bind(Program.Config);
        }

        public static string ConnectionString { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<FortDbContext>(options => options.UseMySql(ConnectionString, b => b.MigrationsAssembly("Fort")));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddScoped<MapUserService>();
            services.AddScoped<MapTeamService>();
            services.AddScoped<MapAdminService>();
            services.AddScoped<CurrentPlayerService>();
            services.AddSingleton<RealPositionService>();
            services.AddSingleton<RoundService>();
            services.AddSingleton<CommService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseMiddleware<CurrentPlayerMiddleware>();
            app.UseMiddleware<LoggerMiddleware>();
            app.UseMiddleware<WSMiddleware>();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "MapCreator",
                    template: "{code}/MapCreator/{action=Index}",
                    defaults: new { controller = "MapCreator" });

                routes.MapRoute(
                    name: "Play",
                    template: "{code=}/{action=Login}",
                    defaults: new { controller = "Play" });
            });

            Logger.Configure(configuration.GetSection("Logger"));
            app.ApplicationServices.GetService<RoundService>().Setup(configuration.GetSection("StartingPositions"));
        }
    }
}

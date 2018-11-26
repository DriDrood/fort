using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Services;
using Fort.Utils.Logger;
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseMiddleware<LoggerMiddleware>();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "MapCreator",
                    template: "Admin/MapCreator/{action=Index}",
                    defaults: new { controller = "MapCreator" });

                routes.MapRoute(
                    name: "Admin",
                    template: "Admin/{code=}/{action=Login}",
                    defaults: new { controller = "PlayAdmin" });

                routes.MapRoute(
                    name: "Team",
                    template: "Team/{code=}/{action=Login}/",
                    defaults: new { controller = "PlayTeam" });

                routes.MapRoute(
                    name: "User",
                    template: "{code=}/{action=Login}",
                    defaults: new { controller = "PlayUser" });
            });
        }
    }
}

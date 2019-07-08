using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fort.Database;
using Fort.Module;
using Fort.Module.Comm;
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

            services.AddSingleton<RedisService>();
            services.AddSingleton<RoundService>();
            services.AddSingleton<CommService>();
            services.AddSingleton<PlayerService>();
            services.AddSingleton<ActionService>();
            services.AddScoped<ContextService>();
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
                MigrateDatabase(app);
            }

            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseMiddleware<WSTestMiddleware>();
            app.UseMiddleware<CurrentPlayerMiddleware>();
            app.UseMiddleware<LoggerMiddleware>();
            app.UseMiddleware<WSMiddleware>();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Guid",
                    template: "GenerateGuid",
                    defaults: new { controller = "Play", action = "GenerateGuid" });

                routes.MapRoute(
                    name: "MapCreator",
                    template: "{code}/MapCreator/{action=Index}",
                    defaults: new { controller = "MapCreator" });

                routes.MapRoute(
                    name: "Play",
                    template: "{code}/{action=Map}",
                    defaults: new { controller = "Play" });

                routes.MapRoute(
                    name: "Login",
                    template: "",
                    defaults: new { controller = "Play", action = "Login" });
            });


            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ContextService>())
                {
                    Logger.Configure(configuration.GetSection("Logger"));
                    app.ApplicationServices.GetService<RedisService>().Init(configuration.GetSection("Redis"));
                    app.ApplicationServices.GetService<CommService>().Init(context, app.ApplicationServices.GetService<RedisService>());
                    app.ApplicationServices.GetService<RoundService>().Init(context, configuration.GetSection("Round"));
                    app.ApplicationServices.GetService<ActionService>().Init();
                }
            }
        }

        private static void MigrateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ContextService>())
                {
                    context.Database.Database.Migrate();
                }
            }
        }
    }
}

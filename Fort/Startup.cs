using System;
using Fort.Database;
using Fort.Managers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Fort
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = new ConfigManager(configuration);
        }

        public static ConfigManager Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<FortDbContext>(options => options.UseMySql(Configuration.ConnectionString));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Configuration.JwtToken.PrivateKey))
                    };
                });

            // services.AddScoped<MapUserService>();
            // services.AddScoped<MapTeamService>();
            // services.AddScoped<MapAdminService>();
            // services.AddScoped<CurrentPlayerService>();
            // services.AddSingleton<RoundService>();
            // services.AddSingleton<ActionService>();
            // services.AddSingleton<CommService>();
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
                MigrateDatabase(app);
            }

            // app.UseWebSockets();
            // app.UseMiddleware<WSTestMiddleware>();
            // app.UseMiddleware<CurrentPlayerMiddleware>();
            // app.UseMiddleware<LoggerMiddleware>();
            // app.UseMiddleware<WSMiddleware>();
            app.UseMvc(routes => routes.MapRoute("default", "api/{controller}/{action}/{id?}"));

            // Logger.Configure(configuration.GetSection("Logger"));
            // app.ApplicationServices.GetService<RoundService>().Setup(configuration.GetSection("StartingPositions"));
        }

        private static void MigrateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<FortDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}

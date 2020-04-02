using System;
using System.Security.Claims;
using Fort.Database;
using Fort.Managers;
using Fort.Middlewares;
using Fort.Services;
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
            ConfigManager.Setup(configuration);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<FortDbContext>(options => options.UseMySql(ConfigManager.ConnectionString));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(ConfigManager.JwtToken.PrivateKey))
                    };
                });
            services.AddAuthorization(opt => opt.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin")));

            services.AddSingleton<LifecycleService>();
            services.AddScoped<Context>();
            services.AddScoped<MapManager>();
            services.AddScoped<TurnManager>();
            services.AddScoped<UserManager>();
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
                MigrateDatabase(app);
            }

            app.UseAuthentication();
            app.UseMiddleware<ContextMiddleware>();
            // app.UseWebSockets();
            // app.UseMiddleware<WSTestMiddleware>();
            // app.UseMiddleware<CurrentPlayerMiddleware>();
            // app.UseMiddleware<LoggerMiddleware>();
            // app.UseMiddleware<WSMiddleware>();
            app.UseMvc(routes => routes.MapRoute("default", "api/{controller}/{action}/{id?}"));
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

using Fort.Comm;
using Fort.Database;
using Fort.Extensions;
using Fort.Managers;
using Fort.Middlewares;
using Fort.Models;
using Fort.Services;
using Fort.Utils.Logger;
using Fort.Utils.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

      services.AddSingleton<Logger>();
      services.AddSingleton<LifecycleService>();
      services.AddSingleton<ConnectionsService>();
      services.AddSingleton<DoneService>();

      services.AddScoped<ConnectionServiceProviderHolder>();
      services.AddConnectionScoped<WsConnection>();
      services.AddConnectionScoped<JwtUser>();

      services.AddScoped<MessageContext>();
      services.AddScoped<MapManager>();
      services.AddScoped<TurnManager>();
      services.AddScoped<UserManager>();
      services.AddScoped<PlayerController>();
      services.AddScoped<AdminController>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseWebSockets();
      app.UseMiddleware<WsConnectorMiddleware>();
      app.UseMiddleware<LoggerMiddleware>();
      app.UseMiddleware<WsResponderMiddleware>();
      app.UseMiddleware<WsParserMiddleware>();
      app.UseMiddleware<WsAuthorizationMiddleware>();
      app.UseMiddleware<WsRouterMiddleware>();

      SetupServices(app, env);
    }

    private static void SetupServices(IApplicationBuilder app, IHostingEnvironment env)
    {
      using (var serviceScope = app.ApplicationServices
        .GetRequiredService<IServiceScopeFactory>()
        .CreateScope())
      {
        var db = serviceScope.ServiceProvider.GetService<FortDbContext>();

        if (!env.IsDevelopment())
        {
          db.Database.Migrate();
        }

        app.ApplicationServices.GetService<Logger>().Setup(ConfigManager.Logger);
        app.ApplicationServices.GetService<LifecycleService>().Init(db);
      }
    }
  }
}

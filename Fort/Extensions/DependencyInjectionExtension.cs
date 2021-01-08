using Microsoft.Extensions.DependencyInjection;
using Fort.Services;
using System.Linq;
using System;

namespace Fort.Extensions
{
  public static class DependencyInjectionExtension
  {
    public static void AddConnectionScoped<TService>(this IServiceCollection self) where TService : class
    {
      self.AddScoped<TService>(sp => {
        // get parrent service
        var connectionServiceProvider = sp.GetService<ConnectionServiceProviderHolder>().ServiceProvider;
        if (connectionServiceProvider != null)
          return connectionServiceProvider.GetService<TService>();

        // create new instance
        var type = typeof(TService);
        var constructor = type.GetConstructors()[0];
        var param = constructor.GetParameters().Select(p => sp.GetService(p.ParameterType));
        var instance = (TService)Activator.CreateInstance(type, param.ToArray());
        return instance;
      });
    }

    public static IServiceScope CreateMessageScope(this IServiceProvider self)
    {
      var scope = self.GetService<IServiceScopeFactory>().CreateScope();
      scope.ServiceProvider.GetService<ConnectionServiceProviderHolder>().ServiceProvider = self;
      return scope;
    }
  }
}
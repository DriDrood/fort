using System;
using System.Linq;
using System.Threading.Tasks;
using Fort.Extensions;
using Fort.Models;
using Microsoft.AspNetCore.Http;

namespace Fort.Utils.WebSocket
{
  public class WsRouterMiddleware
  {
    public WsRouterMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    private RequestDelegate _next;

    public Task Invoke(HttpContext _, MessageContext context, IServiceProvider serviceProvider, WsConnection connection)
    {
      // get route
      (var controllerName, var methodName) = ParseRoute(context.Route, '/');

      // get controller
      var controllerType = Type.GetType($"Fort.Comm.{controllerName.ToCamelCase()}Controller")
        ?? throw new Exception($"Unknown method namespace - {controllerName}");
      var controller = serviceProvider.GetService(controllerType);

      // get param
      var method = controllerType.GetMethod(methodName.ToCamelCase())
        ?? throw new Exception($"Unknown method - {methodName}");
      var paramType = method.GetParameters().FirstOrDefault()?.ParameterType;
      var param = paramType != null ? context.Data.ToObject(paramType) : null;

      // RUN
      try
      {
        context.Response = param != null
          ? method.Invoke(controller, new object[] { param })
          : method.Invoke(controller, new object[0]);
      }
      catch (Exception ex)
      {
        throw ex.InnerException;
      }
      
      return Task.CompletedTask;
    }

    public (string, string) ParseRoute(string route, char splitter)
    {
      var splitted = route.Split(splitter);
      if (splitted.Length != 2)
        throw new ArgumentException("Wrong route");
      return (splitted[0], splitted[1]);
    }
  }
}
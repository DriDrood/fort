using System;
using System.Threading.Tasks;
using Fort.Services;
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

    public async Task Invoke(HttpContext _, MessageContext context, IServiceProvider serviceProvider, WsConnection connection)
    {
      // get route
      (var controllerName, var methodName) = ParseRoute(context.Route, '/');

      // get controller
      var controllerType = Type.GetType($"Fort.Comm.{controllerName}Controller");
      var controller = serviceProvider.GetService(controllerType);

      // get param
      var method = controllerType.GetMethod(methodName);
      var paramType = method.GetParameters()[0].ParameterType;
      var param = context.Data.ToObject(paramType);

      // RUN
      var result = method.Invoke(controller, new object[] { param });

      if (result != null)
      {
        await connection.Send(context.Route, result);
      }
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
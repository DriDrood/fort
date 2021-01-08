using System.Security.Authentication;
using System.Threading.Tasks;
using Fort.Models;
using Fort.Services;
using Microsoft.AspNetCore.Http;

namespace Fort.Middlewares
{
  public class WsAuthorizationMiddleware
  {
    public WsAuthorizationMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    private RequestDelegate _next;

    public Task Invoke(HttpContext _, MessageContext messageContext, JwtUser jwtUser)
    {
      // only login can be annonymous
      if (messageContext.Route != "player/login" && jwtUser.IsAnnonymous)
        throw new AuthenticationException("Invalid token");
        
      // admin endpoints
      if (messageContext.Route.StartsWith("admin/") && jwtUser.Role != "Admin")
        throw new AuthenticationException("Only admin can run this task!");

      return _next.Invoke(_);
    }
  }
}
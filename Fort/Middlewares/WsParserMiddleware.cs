using System;
using System.Text;
using System.Threading.Tasks;
using Fort.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Fort.Middlewares
{
  public class WsParserMiddleware
  {
    public WsParserMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    private RequestDelegate _next;

    public Task Invoke(HttpContext _, MessageContext context, JwtUser jwtUser)
    {
      var param = JsonConvert.DeserializeObject<WebSocketParam>(context.InputMessage);

      // fill context
      context.Route = param.Route;
      context.Data = param.Data;

      // fill jwtUser with newer
      if (jwtUser.Token != param.JwtToken)
      {
        var tokenData = Convert.FromBase64String(param.JwtToken.Split('.')[1]);
        var tokenDataString = Encoding.UTF8.GetString(tokenData);
        var UpdatedUser = JsonConvert.DeserializeObject<JwtUser>(tokenDataString);

        jwtUser.Token = param.JwtToken;
        jwtUser.Email = UpdatedUser.Email;
        jwtUser.Given_Name = UpdatedUser.Given_Name;
        jwtUser.Role = UpdatedUser.Role;
        jwtUser.NotValidBefore = UpdatedUser.NotValidBefore;
        jwtUser.ExpirationTime = UpdatedUser.ExpirationTime;
        jwtUser.IssuedAt = UpdatedUser.IssuedAt;
      }

      return _next.Invoke(_);
    }
  }
}
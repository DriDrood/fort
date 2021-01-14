using System;
using System.Text;
using System.Threading.Tasks;
using Fort.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RingoRegistration.backend.Utils;

namespace Fort.Middlewares
{
  public class WsParserMiddleware
  {
    public WsParserMiddleware(RequestDelegate next)
    {
      _next = next;
      _jwtHandler = new JwtHandler();
    }

    private RequestDelegate _next;
    private readonly JwtHandler _jwtHandler;

    public Task Invoke(HttpContext _, MessageContext context, JwtUser jwtUser)
    {
      var param = JsonConvert.DeserializeObject<WebSocketParam>(context.InputMessage);

      // fill context
      context.MessageId = param.MessageId;
      context.Route = param.Route;
      context.Data = param.Data;

      // authenticate
      if (jwtUser.Token != param.JwtToken)
        _jwtHandler.ValidateUpdateJwt(param.JwtToken, jwtUser);

      return _next.Invoke(_);
    }
  }
}
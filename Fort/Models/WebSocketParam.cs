using System;
using Newtonsoft.Json.Linq;

namespace Fort.Models
{
  public class WebSocketParam
  {
    public Guid MessageId { get; set; }
    public string Route { get; set; }
    public JToken Data { get; set; }
    public string JwtToken { get; set; }
  }
}
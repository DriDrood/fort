using System;
using Newtonsoft.Json.Linq;

namespace Fort.Models
{
  public class MessageContext
  {
    public Guid MessageId { get; set; }
    
    public string Route { get; set; }
    public JToken Data { get; set; }
    public object Response { get; set; }
    
    public string InputMessage { get; set; }
  }
}
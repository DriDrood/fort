using Newtonsoft.Json.Linq;

namespace Fort.Services
{
  public class MessageContext
  {
    public string Route { get; set; }
    public JToken Data { get; set; }
    public object Response { get; set; }
    
    public string InputMessage { get; set; }
  }
}
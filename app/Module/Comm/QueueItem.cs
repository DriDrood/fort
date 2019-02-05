using System;
using Newtonsoft.Json.Linq;

namespace app.Module.Comm
{
    public class QueueItem
    {
        public QueueItem(TimeSpan lifetime)
        {
            LifetimeEndsAt = DateTime.UtcNow + lifetime;
        }
        
        public string Method { get; set; }
        public JToken Message { get; set; }
        public DateTime LifetimeEndsAt { get; }
    }
}
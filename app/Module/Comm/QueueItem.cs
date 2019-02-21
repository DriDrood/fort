using System;
using Newtonsoft.Json.Linq;

namespace Fort.Module.Comm
{
    public class QueueItem
    {
        public QueueItem()
        {
            CreatedAt = DateTime.UtcNow;
        }
        public string Data { get; set; }
        public Lifetime Lifetime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
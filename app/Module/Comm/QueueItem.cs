using System;
using Newtonsoft.Json.Linq;

namespace Fort.Module.Comm
{
    public class QueueItem
    {
        public QueueItem()
        {
            Id = Guid.NewGuid();
        }
        
        public Guid Id { get; set; }
        public string Data { get; set; }
        public Lifetime Lifetime { get; set; }
    }
}
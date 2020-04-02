using System;

namespace Fort.Models
{
    public class OrderData
    {
        public Guid SourceId { get; set; }
        public Guid TargetId { get; set; }
        public int Amount { get; set; }
    }
}
using System;

namespace Fort.Models.Params
{
    public class OrderParams
    {
        public Guid SourceId { get; set; }
        public Guid TargetId { get; set; }
        public int Amount { get; set; }
    }
}
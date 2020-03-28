using System;

namespace Fort.Database.Entities
{
    public class Road
    {
        public Guid Id { get; set; }

        public Guid SourceId { get; set; }
        public City Source { get; set; }

        public Guid TargetId { get; set; }
        public City Target { get; set; }
    }
}
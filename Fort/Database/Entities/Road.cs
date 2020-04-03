using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fort.Database.Entities
{
    public class Road
    {
        public Guid SourceId { get; set; }
        public City Source { get; set; }

        public Guid TargetId { get; set; }
        public City Target { get; set; }

        /// <summary>
        /// Source should be always smaller than target
        /// </summary>
        [NotMapped]
        public string Id
            => $"{SourceId}__{TargetId}";
    }
}
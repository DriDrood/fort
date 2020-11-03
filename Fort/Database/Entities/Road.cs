using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fort.Database.Entities
{
  public class Road
  {
    public Guid SourceId { get; set; } // key
    public City Source { get; set; }

    public Guid TargetId { get; set; } // key
    public City Target { get; set; }

    public IEnumerable<Order> Orders { get; set; } = new HashSet<Order>();

    /// <summary>
    /// Source should be always smaller than target
    /// </summary>
    [NotMapped]
    public string Id
        => $"{SourceId}__{TargetId}";
  }
}
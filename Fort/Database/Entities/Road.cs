using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fort.Database.Entities
{
  public class Road
  {
    public Guid StCityId { get; set; } // key
    public City StCity { get; set; }

    public Guid NdCityId { get; set; } // key
    public City NdCity { get; set; }

    public IEnumerable<Order> Orders { get; set; } = new HashSet<Order>();

    /// <summary>
    /// Source should be always smaller than target
    /// </summary>
    [NotMapped]
    public string Id
        => $"{StCityId}__{NdCityId}";
  }
}
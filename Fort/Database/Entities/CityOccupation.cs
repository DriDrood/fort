using System;
using System.Collections.Generic;

namespace Fort.Database.Entities
{
  public class CityOccupation
  {
    public int Army { get; set; }

    public Guid CityId { get; set; } // key
    public City City { get; set; }

    public Guid? OwnerId { get; set; }
    public User Owner { get; set; }

    public int TurnId { get; set; } // key
    public Turn Turn { get; set; }

    public ICollection<Order> SourceForOrders { get; set; } = new HashSet<Order>();
    public ICollection<Order> TargetForOrders { get; set; } = new HashSet<Order>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}
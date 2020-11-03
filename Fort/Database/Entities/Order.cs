using System;

namespace Fort.Database.Entities
{
  public class Order
  {
    public int Amount { get; set; }
    public bool IsReverseDirection { get; set; } // key

    public Guid SourceCityId { get; set; } // key
    public City SourceCity { get; set; }

    public Guid TargetCityId { get; set; } // key
    public City TargetCity { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public int TurnId { get; set; } // key
    public Turn Turn { get; set; }

    public Road Road { get; set; }

    public CityOccupation TargetCityOccupation { get; set; }
    public CityOccupation SourceCityOccupation { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}
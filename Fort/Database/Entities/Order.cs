using System;

namespace Fort.Database.Entities
{
  public class Order
  {
    public int Amount { get; set; }
    public bool StIsSource { get; set; } // key

    public Guid StCityId { get; set; } // key
    public City StCity { get; set; }

    public Guid NdCityId { get; set; } // key
    public City NdCity { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public int TurnId { get; set; } // key
    public Turn Turn { get; set; }

    public Road Road { get; set; }

    public CityOccupation StCityOccupation { get; set; }
    public CityOccupation NdCityOccupation { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}
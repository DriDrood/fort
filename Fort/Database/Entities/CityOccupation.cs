using System;

namespace Fort.Database.Entities
{
    public class CityOccupation
    {
        public Guid Id { get; set; }

        public int Army { get; set; }
        public Guid CityId { get; set; }
        public City City { get; set; }
        public Guid OwnerId { get; set; }
        public User Owner { get; set; }
        public int TurnId { get; set; }
        public Turn Turn { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
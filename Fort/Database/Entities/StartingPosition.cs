using System;

namespace Fort.Database.Entities
{
    public class StartingPosition
    {
        public Guid CityId { get; set; } // key
        public City City { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public int? Army { get; set; }
    }
}
using System;

namespace Fort.Database.Entities
{
    public class StartingPosition
    {
        public Guid Id { get; set; }
        
        public Guid CityId { get; set; }
        public City City { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public int? Army { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace Fort.Database.Entities
{
    public class Turn
    {
        public int Id { get; set; }

        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }

        public ICollection<CityOccupation> CityOccupations { get; set; } = new HashSet<CityOccupation>();
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
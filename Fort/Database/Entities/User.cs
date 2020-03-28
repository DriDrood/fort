using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Fort.Database.Entities
{
    public class User : IdentityUser<Guid>
    {
        [StringLength(100)]
        public string ImageUrl { get; set; }
        public bool IsAdmin { get; set; }

        public Guid TeamId { get; set; }
        public Team Team { get; set; }

        public ICollection<CityOccupation> CityOccupations { get; set; } = new HashSet<CityOccupation>();
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public ICollection<StartingPosition> StartingPositions { get; set; } = new HashSet<StartingPosition>();
    }
}
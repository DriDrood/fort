using System;
using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class StartingPosition
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(5)]
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid CityId { get; set; }
        public City City { get; set; }
        public int Army { get; set; }
    }
}
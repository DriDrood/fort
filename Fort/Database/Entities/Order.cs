using System;
using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class Order
    {
        public Guid Id { get; set; }

        public int Amount { get; set; }

        public Guid SourceCityId { get; set; }
        public City SourceCity { get; set; }

        public Guid TargetCityId { get; set; }
        public City TargetCity { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public int TurnId { get; set; }
        public Turn Turn { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class Turn
    {
        public int Id { get; set; }

        public int Amount { get; set; }
        public int? ModifiedAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(5)]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        public int SourceCityId { get; set; }
        public virtual City SourceCity { get; set; }

        public int TargetCityId { get; set; }
        public virtual City TargetCity { get; set; }

        public int RoundId { get; set; }
        public virtual Round Round { get; set; }
    }
}
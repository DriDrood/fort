using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fort.Database.Entities
{
    public class Round
    {
        public Round()
        {
            Turns = new HashSet<Turn>();
            ReadyUserIds = new HashSet<string>();
        }

        public int Id { get; set; }

        public int RoundNumber { get; set; }
        [StringLength(1000)]
        public string Note { get; set; }

        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }

        public virtual ICollection<Turn> Turns { get; set; }

        [NotMapped]
        public HashSet<string> ReadyUserIds { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class Round
    {
        public Round()
        {
            Turns = new HashSet<Turn>();
        }

        public int Id { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        public DateTime StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }

        public virtual ICollection<Turn> Turns { get; set; }
    }
}
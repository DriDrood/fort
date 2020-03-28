using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fort.Database.Entities
{
    public class Turn
    {
        public int Id { get; set; }

        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
    }
}
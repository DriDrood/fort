using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class Team
    {
        public Guid Id { get; set; }

        public double ArmyStrengthCoef { get; set; } = 1;
        public double PopulationGrowthCoef { get; set; } = 1;
        [Required]
        [StringLength(10)]
        public string Color { get; set; }
        [Required]
        [StringLength(10)]
        public string ColorLight { get; set; }

        public ICollection<User> Members { get; set; } = new HashSet<User>();
    }
}
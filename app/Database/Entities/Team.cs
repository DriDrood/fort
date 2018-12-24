using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class Team : Player
    {
        public Team()
        {
            Members = new HashSet<User>();
        }

        [StringLength(50)]
        public string Name { get; set; }
        public double ArmyStrengthCoef { get; set; }
        public int? PopulationGrowth { get; set; }

        public virtual ICollection<User> Members { get; set; }
    }
}
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

        public double ArmyStrengthCoef { get; set; }
        public int? PopulationGrowth { get; set; }

        public virtual ICollection<User> Members { get; set; }
    }
}
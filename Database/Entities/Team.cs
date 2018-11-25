using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class Team
    {
        public Team()
        {
            Members = new HashSet<User>();
        }

        [StringLength(5)]
        public string Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }

        public virtual ICollection<User> Members { get; set; }
    }
}
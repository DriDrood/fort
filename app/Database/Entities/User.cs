using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class User : Player
    {
        public User()
        {
            Cities = new HashSet<City>();
            Turns = new HashSet<Turn>();
        }

        [StringLength(100)]
        public string ImageUrl { get; set; }
        public bool IsAdmin { get; set; }

        [StringLength(5)]
        public string TeamId { get; set; }
        public virtual Team Team { get; set; }

        public virtual ICollection<City> Cities { get; set; }
        public virtual ICollection<Turn> Turns { get; set; }
    }
}
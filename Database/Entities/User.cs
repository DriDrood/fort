using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class User
    {
        public User()
        {
            Cities = new HashSet<City>();
            Turns = new HashSet<Turn>();
        }

        [StringLength(5)]
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }
        [StringLength(100)]
        public string ImageUrl { get; set; }

        [StringLength(5)]
        public string TeamId { get; set; }
        public virtual Team Team { get; set; }

        public virtual ICollection<City> Cities { get; set; }
        public virtual ICollection<Turn> Turns { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class City
    {
        public City()
        {
            SourceToPaths = new HashSet<Path>();
            TargetToPaths = new HashSet<Path>();
            SourceToTurns = new HashSet<Turn>();
            TargetToTurns = new HashSet<Turn>();
        }

        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Army { get; set; }
        public int? Grow { get; set; }

        [StringLength(5)]
        public string OwnerId { get; set; }
        public virtual User Owner { get; set; }

        public virtual ICollection<Path> SourceToPaths { get; set; }
        public virtual ICollection<Path> TargetToPaths { get; set; }
        public virtual ICollection<Turn> SourceToTurns { get; set; }
        public virtual ICollection<Turn> TargetToTurns { get; set; }
    }
}
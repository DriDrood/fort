using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public class StartingPosition
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(5)]
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public int CityId { get; set; }
        public virtual City City { get; set; }
        public int? Army { get; set; }
    }
}
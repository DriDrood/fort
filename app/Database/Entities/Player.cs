using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public abstract class Player
    {
        [StringLength(5)]
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public abstract bool IsUser();
        public abstract string GetTeamId();
    }
}
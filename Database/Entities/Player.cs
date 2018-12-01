using System.ComponentModel.DataAnnotations;

namespace Fort.Database.Entities
{
    public abstract class Player
    {
        [StringLength(5)]
        public string Id { get; set; }
    }
}
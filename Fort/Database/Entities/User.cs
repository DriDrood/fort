using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Fort.Database.Entities
{
    public class User : IdentityUser<Guid>
    {
        [StringLength(100)]
        public string ImageUrl { get; set; }
        public bool IsAdmin { get; set; }

        public Guid TeamId { get; set; }
        public Team Team { get; set; }
    }
}
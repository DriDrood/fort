using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Fort.Utils;

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
        public int LastRoundReady { get; set; }

        [StringLength(5)]
        public string TeamId { get; set; }
        public virtual Team Team { get; set; }

        public virtual ICollection<City> Cities { get; set; }
        public virtual ICollection<Turn> Turns { get; set; }

        public override bool IsUser() => true;
        public override string GetTeamId() => TeamId;
        public override string GetColor()
        {
            var teamColor = Colors.HexColorToInt(Team.Color);
            var myColor = Colors.Lighter(teamColor, 1.8);
            return $"rgb({myColor[0]}, {myColor[1]}, {myColor[2]})";
        }

    }
}
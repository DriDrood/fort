using System;
using Fort.Database.Entities;

namespace app.Models
{
    public class CurrentRound
    {
        public CurrentRound(Round round)
        {
            Id = round.Id;
            RoundNumber = round.RoundNumber;
            StartsAt = round.StartsAt.Value;
            EndsAt = round.EndsAt.Value;
        }

        public int Id { get; set; }
        public int RoundNumber { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }

        public static explicit operator Round(CurrentRound round)
        {
            return new Round();
        }
        public static implicit operator CurrentRound(Round round)
        {
            return new CurrentRound(round);
        }
    }
}
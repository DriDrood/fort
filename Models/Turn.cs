using Fort.Services;

namespace Fort.Models
{
    public class Turn
    {
        public int Id { get; set; }

        public int Order { get; set; }

        public Fortress From { get; set; }
        public Fortress To { get; set; }

        public int Amount { get; set; }

        public void Play()
        {
            From.Population -= Amount;

            // ally
            if (To.Owner.Team == From.Owner.Team)
                To.Population += Amount;

            // enemy
            else
            {
                To.Population -= Amount;

                // ofender wins battle
                if (To.Population < 0)
                {
                    To.Population = -To.Population;
                    To.Owner = From.Owner;
                }
            }
        }
    }
}
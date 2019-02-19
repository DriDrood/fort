using System.Collections.Generic;
using System.Linq;
using Fort.Database.Entities;

namespace Fort.Module.Army
{
    public class ArmyTeamService : ArmyService
    {
        public ArmyTeamService(ContextService context, RoundService roundService) : base(context, roundService)
        {
        }

        protected override int GetArmy(City city)
        {
            if (city.Owner.TeamId == _context.CurrentPlayer.Id)
                return city.Army;
                
            return -1;
        }

        protected override string GetCityColor(City city)
        {
            return city.Owner.Team.Color;
        }

        protected override string GetImage(City city)
        {
            // team
            if (city.Owner.TeamId == _context.CurrentPlayer.Id)
                return city.Owner.ImageUrl;

            // near enemy
            if (city.SourceToPaths.Any(p => p.Target.Owner.TeamId == _context.CurrentPlayer.Id)
                || city.TargetToPaths.Any(p => p.Source.Owner.TeamId == _context.CurrentPlayer.Id))
                return city.Owner.ImageUrl;
                
            return null;
        }

        protected override IEnumerable<Turn> GetVisibleTurn()
        {
            return new Turn[0];
        }
    }
}
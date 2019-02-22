using System.Collections.Generic;
using System.Linq;
using Fort.Database.Entities;

namespace Fort.Module.Army
{
    public class ArmyUserService : ArmyService
    {
        public ArmyUserService(ContextService context) : base(context)
        {
        }

        protected override int GetArmy(City city)
        {
            if (city.Owner == _context.CurrentPlayer
                || city.Owner?.TeamId == _context.CurrentPlayer.GetTeamId())
                return city.Army;

            return -1;
        }

        protected override string GetCityColor(City city)
        {
            if (city.OwnerId == _context.CurrentPlayer.Id)
                return "cyan";

            return city.Owner?.Team?.Color ?? "gray";
        }

        protected override string GetImage(City city)
        {
            // user or team
            if (city.OwnerId == _context.CurrentPlayer.Id
                || city.Owner?.TeamId == _context.CurrentPlayer.GetTeamId())
                return city.Owner?.ImageUrl;

            // near enemy
            if (city.SourceToPaths.Any(p => p.Target.Owner?.TeamId == _context.CurrentPlayer.GetTeamId())
                || city.TargetToPaths.Any(p => p.Source.Owner?.TeamId == _context.CurrentPlayer.GetTeamId()))
                return city.Owner?.ImageUrl;

            // other
            return null;
        }

        protected override IEnumerable<Turn> GetVisibleTurn()
        {
            return _context.Database.Turns.Where(t => t.UserId == _context.CurrentPlayer.Id && t.RoundId == _roundService.CurrentRound.Id);
        }
    }
}